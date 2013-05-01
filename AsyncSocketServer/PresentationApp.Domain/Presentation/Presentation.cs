using System;
using System.Collections.Generic;
using PresentationApp.Domain.Account;

namespace PresentationApp.Domain.Presentation
{
    public class Presentation : LogTimeEntity
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

        public virtual bool IsHostConnected { get; set; }
    }
}