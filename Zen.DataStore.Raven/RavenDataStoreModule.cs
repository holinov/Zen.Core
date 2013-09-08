using System;
using Autofac;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;

namespace Zen.DataStore.Raven
{
    /// <summary>
    ///     Регистрация DataStorage и фабрики сессий
    /// </summary>
    public class RavenDataStoreModule : Module
    {
        private readonly IDocumentStore _ds;
        private bool _useCreationConverter = true;

        /// <summary>
        ///     Временная БД на порту 8080
        /// </summary>
        public RavenDataStoreModule() : this(@"http://localhost:8080", "TempDb")
        {
        }

        /// <summary>
        ///     Подключение с хранилищю по строке подключения
        /// </summary>
        /// <param name="connectionStringName">Имя строки подключения в конфиге</param>
        public RavenDataStoreModule(string connectionStringName)
        {
            ConnectionStringName = connectionStringName;
        }

        /// <summary>
        ///     Подключение к БД по URL и имени базы
        /// </summary>
        /// <param name="url">Адрес сервера</param>
        /// <param name="defaultDb">Имя базы</param>
        public RavenDataStoreModule(string url, string defaultDb)
        {
            Url = url;
            DefaultDatabase = defaultDb;
            ConnectionStringName = null;
        }

        /// <summary>
        ///     Использование сконфигурированного хранилища
        ///     Должно быть уже проинициализировано
        /// </summary>
        /// <param name="ds">Хранилище документов</param>
        public RavenDataStoreModule(IDocumentStore ds)
        {
            _ds = ds;
        }

        public string ConnectionStringName { get; set; }
        public string Url { get; set; }
        public string DefaultDatabase { get; set; }

        public bool UseCreationConverter
        {
            get { return _useCreationConverter; }
            set { _useCreationConverter = value; }
        }

        public bool CreateIndexes { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            /* builder
                .RegisterAssemblyTypes(typeof(IReport).Assembly)
                .AssignableTo<IReport>()
                .As<IReport>();*/

            builder.RegisterType<AutofacCreationConverter>().AsSelf().SingleInstance();

            builder.Register(context => InitDocumentStore(context.Resolve<AutofacCreationConverter>()))
                   .AsSelf()
                   .As<IDocumentStore>()
                   .SingleInstance()
                   .OnRelease(x => { if (!x.WasDisposed) x.Dispose(); });

            builder
                .Register(context => context.Resolve<IDocumentStore>().OpenSession())
                .As<IDocumentSession>()
                .InstancePerDependency();
        }

        private IDocumentStore InitDocumentStore(AutofacCreationConverter converter)
        {
            IDocumentStore ds;
            if (_ds != null)
                ds = _ds;
            else if (ConnectionStringName != null)
            {
                ds = new DocumentStore
                    {
                        ConnectionStringName = ConnectionStringName,
                        ResourceManagerId = Guid.NewGuid()
                    };
                ds.Initialize();
            }
            else if (!string.IsNullOrEmpty(Url))
            {
                var store = new DocumentStore
                    {
                        Url = Url,
                        ResourceManagerId = Guid.NewGuid()
                    };
                if (!string.IsNullOrEmpty(DefaultDatabase))
                    store.DefaultDatabase = DefaultDatabase;
                store.Initialize();

                ds = store;
            }
            else
            {
                ds = new DocumentStore
                    {
                        Url = "http://localhost:9901",
                        ResourceManagerId = Guid.NewGuid()
                    };
                ds.Initialize();
            }

            if (converter != null && UseCreationConverter)
            {
                ds.Conventions.CustomizeJsonSerializer += s => s.Converters.Add(converter);
            }

            ds.Conventions.DisableProfiling = true;
            ds.Conventions.JsonContractResolver = new RecordClrTypeInJsonContractResolver();
            ds.Conventions.CustomizeJsonSerializer += s => s.TypeNameHandling = TypeNameHandling.Arrays;

            if (CreateIndexes)
            {
                IndexCreation.CreateIndexes(ThisAssembly, ds);
            }
            //ds.DatabaseCommands.EnsureDatabaseExists()            
            //global::Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(RegionTrajectoryIndex).Assembly, ds);

            return ds;
        }
    }
}