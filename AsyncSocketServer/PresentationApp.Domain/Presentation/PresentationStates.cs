using System.Collections.Generic;

namespace PresentationApp.Domain.Presentation
{
    public class PresentationStates : LogTimeEntity
    {
        public virtual string PresentationState { get; set; }

        public virtual ICollection<Presentation> Presentations { get; set; }
    }
}