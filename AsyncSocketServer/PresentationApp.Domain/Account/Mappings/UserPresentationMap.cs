using FluentNHibernate.Mapping;

namespace PresentationApp.Domain.Account.Mappings
{
    internal class UserPresentationMap : ClassMap<UserPresentation>
    {
        public UserPresentationMap()
        {
            Id(x => x.Id);
            References(x => x.Presentation);
            References(x => x.Users);
            Map(x => x.PresentationKey);
            Map(x => x.IsUserConnected);
        }
    }
}