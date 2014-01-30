using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using FluentNHibernate.Automapping;
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
        private readonly Action<FluentConfiguration> _configAction;

        public NHibernateDatastoreModule(params Assembly[] mappingAssemblies)
            : this(null, null, mappingAssemblies)
        {

        }

        public NHibernateDatastoreModule(Action<FluentConfiguration> configAction, params Assembly[] mappingAssemblies):this(null, configAction, mappingAssemblies)
        {
            
        }
        public NHibernateDatastoreModule(IPersistenceConfigurer persConf, Action<FluentConfiguration> configAction, params Assembly[] mappingAssemblies)
        {
            _persConf = persConf;
            _configAction = configAction;
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
                        m.AutoMappings.Add(AutoMap.Assembly(mappingAssembly));
                        m.FluentMappings.AddFromAssembly(mappingAssembly);
                    }
                });
            if (ExposeConfiguration)
                cnf.ExposeConfiguration(config => new SchemaExport(config).Create(MakeScript, Export));

            if (_configAction != null)
                _configAction(cnf);

            return cnf.BuildSessionFactory();
        }

        public bool Export { get; set; }

        public bool MakeScript { get; set; }

        public bool ExposeConfiguration { get; set; }
    }
}
