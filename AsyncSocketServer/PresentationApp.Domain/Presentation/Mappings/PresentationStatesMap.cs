using FluentNHibernate.Mapping;

namespace PresentationApp.Domain.Presentation.Mappings
{
    internal class PresentationStatesMap : ClassMap<PresentationStates>
    {
        public PresentationStatesMap()
        {
            Id(x => x.Id);
            Map(x => x.PresentationState);
            HasMany(x => x.Presentations)
                .Inverse()
                .Cascade.All();
            Map(x => x.CreatedDate);
            Map(x => x.UpdatedDate);
        }
    }
}