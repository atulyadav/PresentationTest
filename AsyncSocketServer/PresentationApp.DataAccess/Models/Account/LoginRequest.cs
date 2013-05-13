using System;

namespace PresentationApp.DataAccess.Models.Account
{
    public class LoginRequest
    {
        public virtual string Email { get; set; }

        public virtual string Password { get; set; }

        public virtual Guid PresentationKey { get; set; }
    }
}