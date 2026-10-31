using System;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.DTO
{
    public class UserRegDTO
    {
        public string UserName {get;set;} = string.Empty;
        public string Password {get;set;} = string.Empty;
        public string Email {get;set;} = string.Empty;
    }
    public class UserLogInDTO
    {
        public string Email {get;set;} = string.Empty;
        public string Password {get;set;} = string.Empty;

    }
    public class UserResponseDTO
    {
        public Guid Id {get;set;}
        public string UserName {get;set;} = string.Empty;
        public string Email {get;set;} = string.Empty;
        public ICollection<WorkspaceMember> WorkspaceMembership {get;set;} = new List<WorkspaceMember>();
        public Subscription SubscriptionLevel {get;set;}
        public UserResponseDTO(User user)
        {
            Id = user.Id;
            UserName = user.UserName;
            Email = user.Email;
            WorkspaceMembership = user.WorkspaceMembership;
            SubscriptionLevel = user.SubscriptionLevel;
        }
    }
    public class UserAuthResponseDTO : UserResponseDTO
    {
        public string Token {get;set;}
        public UserAuthResponseDTO(User user, string token) : base(user)
        {
            Token = token;
        }
    }
}