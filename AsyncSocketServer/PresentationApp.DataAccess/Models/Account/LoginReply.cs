using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PresentationApp.Domain.Account;
using PresentationApp.Domain.Presentation;

namespace PresentationApp.DataAccess.Models.Account
{
    public class LoginReply
    {
        public Guid PresentationKey { get; set; }

        public int Status { get; set; }

        public long PresentationId { get; set; }

        public string PresentationName { get; set; }

        public long UserId { get; set; } // this will be either presenter id or user id depending on the status

        public string UserName { get; set; }

        public string Description { get; set; }
    }
}