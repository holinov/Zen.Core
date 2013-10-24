using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Module = Autofac.Module;

namespace Zen.DataStore.NHibernate
{
    public class NHibernateDatastoreModule:Module
    {
        private readonly Assembly[] _mappingAssemblies;
        private readonly IPersistenceConfigurer _persConf;

        public NHibernateDatastoreModule(params Assembly[] mappingAssemblies):this(null,mappingAssemblies)
        {
            
        }
        public NHibernateDatastoreModule(IPersistenceConfigurer persConf, Assembly[] mappingAssemblies)
        {
            _persConf = persConf;
            _mappingAssemblies = mappingAssemblies;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => BuildSessionFactory())
                   .As<ISessionFactory>()
                   .OnRelease(s =>
                       {
                           if (!s.IsClosed) s.Close();
                       })
                   .SingleInstance();
            builder.Register(c => c.Resolve<ISessionFactory>().OpenSession()).As<ISession>().InstancePerDependency();
        }

        private ISessionFactory BuildSessionFactory()
        {
            var cnf = Fluently.Configure();
            cnf.Database(_persConf ?? SQLiteConfiguration.Standard.UsingFile("data.db"));
            cnf.Mappings(m =>
                {
                    foreach (var mappingAssembly in _mappingAssemblies)
                    {
                        m.FluentMappings.AddFromAssembly(mappingAssembly);
                    }
                });
            cnf.ExposeConfiguration(config => new SchemaExport(config).Create(false, true));

            return cnf.BuildSessionFactory();
        }
    }
}
