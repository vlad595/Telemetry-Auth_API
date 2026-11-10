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
    }
    public class UserService: IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public UserService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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
    }
}
