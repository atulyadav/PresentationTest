using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PresentationApp.Domain.Account;
using PresentationApp.Domain.Presentation;

namespace PresentationApp.DataAccess.Models.Presentation
{
    internal class PresentationListItem
    {
        public virtual string Name { get; set; }

        public virtual PresentationStates PresentationStates { get; set; }

        public virtual DateTime StartTime { get; set; }

        public virtual Users Presenter { get; set; }

        public virtual Guid PresenterKey { get; set; }

        public virtual string PresentationFileSequence { get; set; }

        public virtual ICollection<UserPresentation> UserPresentations { get; set; }

        public virtual DateTime EndTime { get; set; }

        public virtual bool Status { get; set; }

        public virtual string Description { get; set; }

        public virtual string Timezone { get; set; }
    }
}