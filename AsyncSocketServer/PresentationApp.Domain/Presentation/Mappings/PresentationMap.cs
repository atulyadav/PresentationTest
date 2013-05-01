﻿using FluentNHibernate.Mapping;

namespace PresentationApp.Domain.Presentation.Mappings
{
    internal class PresentationMap : ClassMap<Presentation>
    {
        public PresentationMap()
        {
            Id(x => x.Id).GeneratedBy.Increment();
            Map(x => x.Name);
            References(x => x.PresentationStates);
            Map(x => x.StartTime);
            References(x => x.Presenter);
            Map(x => x.PresenterKey);
            Map(x => x.PresentationFileSequence);
            HasMany(x => x.UserPresentations)
                .Inverse()
                .Cascade.All();
            Map(x => x.EndTime);
            Map(x => x.Status);
            Map(x => x.Description);
            Map(x => x.IsHostConnected);
        }
    }
}