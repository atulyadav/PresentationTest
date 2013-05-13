using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PresentationApp.DataAccess.Models.Presentation
{
    public class CreatePresentationRequest
    {
        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        [DataType(DataType.DateTime)]
        public virtual DateTime StartTime { get; set; }

        [DataType(DataType.DateTime)]
        public virtual DateTime EndTime { get; set; }

        public virtual string Description { get; set; }

        public virtual int flag { get; set; }

        public virtual string Timezone { get; set; }

        public virtual IList<TimeZoneInfo> TZInfo { get; set; }

        public CreatePresentationRequest()
        {
            ReadOnlyCollection<TimeZoneInfo> tz = TimeZoneInfo.GetSystemTimeZones();
            TZInfo = tz.ToList<TimeZoneInfo>();
        }
    }
}