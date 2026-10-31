using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telemetry_Auth_API.Data;
using Telemetry_Auth_API.DTO;
using Telemetry_Auth_API.Exceptions;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.Services
{
    public interface IUserService
    {
        public Task<UserResponseDTO> Me(Guid userId);
        public Task<UserResponseDTO> Register(UserRegDTO user);
        public Task<UserResponseDTO> LogIn(UserLogInDTO user);
        public Task<UserResponseDTO> UpgradePlan(Guid userId, Subscription tier);
        public Task<UserResponseDTO> DowngradePlan(Guid userId, Subscription tier);
        public Task<UserResponseDTO> DeleteUser(Guid userId);
        public Task<UserResponseDTO> RestrictUser(Guid userId);
    }
    public class UserService: IUserService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context)
        {
            _context = context;
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
        public async Task<UserResponseDTO> Register(UserRegDTO user)
        {
            User newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = user.UserName,
                Email = user.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                WorkspaceMembership = new List<WorkspaceMember>(),
                SubscriptionLevel = Subscription.Tier3,
                Instances = new List<Instance>()
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return new UserResponseDTO(newUser);
        }
        public async Task<UserResponseDTO> LogIn(UserLogInDTO user)
        {
            var findedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (findedUser == null)
            {
                throw new NotFoundException();
            }

            return new UserResponseDTO(findedUser);
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

            if (user.SubscriptionLevel <= tier)
            {
                throw new NotAllowedException("You cannot upgrade subscription by downgrade method");
            }

            user.SubscriptionLevel = tier;
            await _context.SaveChangesAsync();
            return new UserResponseDTO(user);
        }
        public async Task<UserResponseDTO> DeleteUser(Guid userId)
        {
            var user = await FindUserById(userId);

            _context.Users.Remove(user);
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
    }
}