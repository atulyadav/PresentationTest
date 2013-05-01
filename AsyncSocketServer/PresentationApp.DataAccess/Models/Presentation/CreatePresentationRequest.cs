﻿using System;
using System.ComponentModel.DataAnnotations;

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
    }
}