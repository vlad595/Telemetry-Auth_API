using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Telemetry_Auth_API.Data;
using Telemetry_Auth_API.DTO;
using Telemetry_Auth_API.Exceptions;
using Telemetry_Auth_API.Models;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Telemetry_Auth_API.Services
{
    public interface IUserService
    {
        public Task<UserResponseDTO> Me(Guid userId);
        public Task<UserAuthResponseDTO> Register(UserRegDTO user);
        public Task<UserAuthResponseDTO> LogIn(UserLogInDTO user);

        public Task<UserResponseDTO> UpgradePlan(Guid userId, Subscription tier);
        public Task<UserResponseDTO> DowngradePlan(Guid userId, Subscription tier);
        public Task<UserResponseDTO> GetPlanByAgentId(Guid agentId);

        public Task<UserResponseDTO> DeleteUser(Guid userId, Guid tokenUserId);
        public Task<UserResponseDTO> RestrictUser(Guid userId);

        public Task<List<UserResponseDTO>> GetUsersTelegramIdByAgentId(Guid agentId);
        public Task<UserResponseDTO> GetUserTelegramChatId(Guid userId);
        public Task<string> MakeTelegramCode(Guid userId);
        public Task<UserResponseDTO> StoreTelegramChatId(string code, string chatId);
        public Task<UserResponseDTO> RemoveTelegramChatId(Guid userId);
    }
    public class UserService: IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        public UserService(AppDbContext context, IConfiguration config, IMemoryCache cache)
        {
            _context = context;
            _config = config;
            _cache = cache;
        }
        public async Task<UserResponseDTO> Me(Guid userId)
        {
            var result = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            
            if (result == null)
            {
                throw new NotFoundException();
            }

            UserResponseDTO response = new UserResponseDTO(result);
            return response;
        }
        public async Task<UserResponseDTO> GetPlanByAgentId(Guid agentId)
        {
            var instance = await _context.Instances.FirstOrDefaultAsync(i => i.Id == agentId);
            if (instance == null)
            {
                throw new NotFoundException("Agent does not found");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == instance.OwnerId);
            if(user == null)
            {
                throw new NotFoundException("User does not found");
            }
            return new UserResponseDTO(user);
        }
        public async Task<UserAuthResponseDTO> Register(UserRegDTO user)
        {
            User newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = user.UserName,
                Email = user.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                SubscriptionLevel = Subscription.Tier3,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            string token = await GenerateJwtToken(newUser);

            return new UserAuthResponseDTO(newUser, token);
        }
        public async Task<UserAuthResponseDTO> LogIn(UserLogInDTO user)
        {
            var findedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (findedUser == null)
            {
                throw new NotFoundException();
            }

            if (!BCrypt.Net.BCrypt.Verify(user.Password, findedUser.PasswordHash))
            {
                throw new NotAllowedException("Password is incorrect");
            }

            string token = await GenerateJwtToken(findedUser);

            return new UserAuthResponseDTO(findedUser, token);
        }
        public async Task<UserResponseDTO> UpgradePlan(Guid userId, Subscription tier)
        {
            var user = await FindUserById(userId);

            if (user.SubscriptionLevel <= tier)
            {
                throw new NotAllowedException("You cannot downgrade subscription by upgrade method");
            }

            user.SubscriptionLevel = tier;
            await _context.SaveChangesAsync();
            return new UserResponseDTO(user);
        }
        public async Task<UserResponseDTO> DowngradePlan(Guid userId, Subscription tier)
        {
            var user = await FindUserById(userId);

            if (user.SubscriptionLevel >= tier)
            {
                throw new NotAllowedException("You cannot upgrade subscription by downgrade method");
            }

            user.SubscriptionLevel = tier;
            await _context.SaveChangesAsync();
            return new UserResponseDTO(user);
        }
        public async Task<UserResponseDTO> DeleteUser(Guid userId, Guid tokenUserId)
        {
            var user = await FindUserById(userId);

            if (user == null)
            {
                throw new NotFoundException();
            }
            if (user.Id != tokenUserId)
            {
                throw new NotAllowedException("You cannot delete someone`s account.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return new UserResponseDTO(user);
        }
        public async Task<UserResponseDTO> RestrictUser(Guid userId)
        {
            throw new NotImplementedException();
        }
        private async Task<User> FindUserById(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException();
            }
            return user;
        }
        private async Task<string> GenerateJwtToken(User user)
        {
            var authSection = _config.GetSection("Auth");
            var jwtKey = authSection["Key"]
                ?? throw new InvalidOperationException("Missing configuration value: Auth:Key.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var jwt = new JwtSecurityToken(
                issuer: authSection["Issuer"],
                audience: authSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public async Task<List<UserResponseDTO>> GetUsersTelegramIdByAgentId(Guid agentId)
        {
            var instance = await _context.Instances.Include(i => i.Owner).FirstOrDefaultAsync(i => i.Id == agentId);
            if (instance == null)
            {
                throw new NotFoundException("Agent does not found");
            }

            if (instance.WorkspaceId == null)
            {
                return string.IsNullOrWhiteSpace(instance.Owner.TelegramChatId)
                    ? []
                    : [new UserResponseDTO(instance.Owner)];
            }

            var workspace = await _context.Workspaces
                .Include(w => w.Owner)
                .Include(w => w.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(w => w.Id == instance.WorkspaceId);
            if (workspace == null)
            {
                throw new NotFoundException("Workspace does not found");
            }

            return workspace.Members.Select(m => m.User)
                .Append(workspace.Owner)
                .Where(u => !string.IsNullOrWhiteSpace(u.TelegramChatId))
                .DistinctBy(u => u.Id)
                .Select(u => new UserResponseDTO(u))
                .ToList();
        }
        public async Task<UserResponseDTO> GetUserTelegramChatId(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User does not found");
            }
            if (user.TelegramChatId == null)
            {
                throw new NotFoundException("User don`t have connected telegram");
            }
            return new UserResponseDTO(user);
        }
        public async Task<string> MakeTelegramCode(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User does not found");
            }
            
            _cache.TryGetValue($"userId:{user.Id}", out string? code);
            if (code != null)
            {
                return $"https://t.me/Opentelemetrybot?start={code}";
            }
            
            string newCode = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            _cache.Set($"userId:{user.Id}", newCode, TimeSpan.FromMinutes(10));
            _cache.Set(newCode, user.Id, TimeSpan.FromMinutes(10));

            return $"https://t.me/Opentelemetrybot?start={newCode}";
        }
        public async Task<UserResponseDTO> StoreTelegramChatId(string code, string chatId)
        {
            if (!_cache.TryGetValue(code, out Guid userId))
            {
                throw new NotFoundException("Telegram link code is invalid or expired");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User does not found");
            }

            user.TelegramChatId = chatId;
            await _context.SaveChangesAsync();

            _cache.Remove(code);
            _cache.Remove($"userId:{user.Id}");

            return new UserResponseDTO(user);
        }
        public async Task<UserResponseDTO> RemoveTelegramChatId(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User does not found");
            }
            user.TelegramChatId = null;
            await _context.SaveChangesAsync();
            return new UserResponseDTO(user);
        }
    }
}
