using System;
using System.Collections.Generic;
using PresentationApp.Domain.Account;

namespace PresentationApp.DataAccess.Models.Account
{
    public class CreateOrEditUserRequest
    {
        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string EmailId { get; set; }

        public virtual string Password { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual string UserName { get; set; }

        public virtual bool IsAdmin { get; set; }

        public int flag { get; set; }
    }
}