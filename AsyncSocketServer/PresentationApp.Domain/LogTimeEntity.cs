using System;

namespace PresentationApp.Domain
{
    public class LogTimeEntity : PersistentEntity
    {
        public virtual DateTime CreatedDate { get; set; }

        public virtual DateTime? UpdatedDate { get; set; }
    }
}