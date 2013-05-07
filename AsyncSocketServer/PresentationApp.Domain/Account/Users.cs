using System.Collections.Generic;

namespace PresentationApp.Domain.Account
{
    public class Users : LogTimeEntity
    {
        public virtual string Name { get; set; }

        public virtual string EmailId { get; set; }

        public virtual bool IsActive { get; set; }

        public virtual ICollection<UserPresentation> UserPresentations { get; set; }

        public virtual ICollection<PresentationApp.Domain.Presentation.Presentation> Presentations { get; set; }

        public virtual string Password { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual string UserName { get; set; }

        public virtual bool IsAdmin { get; set; }

        public Users()
        {
            UserPresentations = new List<UserPresentation>();
            Presentations = new List<PresentationApp.Domain.Presentation.Presentation>();
        }
    }
}