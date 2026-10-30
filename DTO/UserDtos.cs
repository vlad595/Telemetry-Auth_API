using System;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.DTO
{
    public class UserRegDTO
    {
        public string UserName {get;set;} = string.Empty;
        public string Password {get;set;} = string.Empty;
        public string Email {get;set;} = string.Empty;
        public string WorkspaceName {get;set;} = string.Empty;
    }
    public class UserLogInDTO
    {
        public string Email {get;set;} = string.Empty;
        public string Password {get;set;} = string.Empty;

    }
    public class UserReponseDTO
    {
        public Guid Id {get;set;}
        public string UserName {get;set;} = string.Empty;
        public string Email {get;set;} = string.Empty;
        public string WorkspaceName {get;set;} = string.Empty;
        public Subscription SubscriptionLevel {get;set;}
        
    }
}