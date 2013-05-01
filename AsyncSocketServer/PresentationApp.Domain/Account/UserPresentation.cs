using System;

namespace PresentationApp.Domain.Account
{
    public class UserPresentation : LogTimeEntity
    {
        public virtual Users Users { get; set; }

        public virtual PresentationApp.Domain.Presentation.Presentation Presentation { get; set; }

        public virtual Guid PresentationKey { get; set; }

        public virtual bool IsUserConnected { get; set; }
    }
}