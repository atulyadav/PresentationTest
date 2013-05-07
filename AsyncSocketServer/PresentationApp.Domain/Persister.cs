using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;

namespace PresentationApp.Domain
{
    public class Persister
    {
        private static ISessionFactory sessionFactory;

        public static ISession session;

        public static ISession Session
        {
            get
            {
                return session;
            }
        }

        public static void createSession()
        {
            if (session != null)
            {
                if (session.IsOpen) session.Close();
                session = null;
            }
            session = sessionFactory.OpenSession();
            session.FlushMode = FlushMode.Commit;
        }

        static Persister()
        {
            sessionFactory = CreateSessionFactory();
        }

        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
              .Database(MsSqlConfiguration.MsSql2008
              .ConnectionString(c => c
                  .FromAppSetting("PresentationAppConnectionString")).ShowSql())
                  .Mappings(m =>
                  {
                      m.FluentMappings.AddFromAssemblyOf<PresentationApp.Domain.Presentation.Presentation>();
                      m.FluentMappings.AddFromAssemblyOf<PresentationApp.Domain.Account.Users>();
                  })
                  .BuildSessionFactory();
        }

        public static IQueryable<T> Query<T>()
        {
            return Session.Query<T>();
        }
    }
}