using FluentNHibernate.Mapping;

namespace PresentationApp.Domain.Account.Mappings
{
    internal class UsersMap : ClassMap<Users>
    {
        public UsersMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.EmailId);
            Map(x => x.IsActive);
            HasMany(x => x.UserPresentations)
                .Inverse()
                .Cascade.All();
            Map(x => x.CreatedDate);
            Map(x => x.UpdatedDate);
            Map(x => x.Password);
            Map(x => x.PhoneNumber);
            Map(x => x.UserName);
            Map(x => x.IsAdmin);
            Map(x => x.IsBlocked);
        }
    }
}