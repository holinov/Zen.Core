using System;
using System.Reflection;
using Autofac;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Database.Server;
using Module = Autofac.Module;

namespace Zen.DataStore.Raven.Embeeded
{
    /// <summary>
    /// Регистрация DataStorage и фабрики сессий
    /// </summary>
    public class RavenEmbeededDataStoreModule : Module
    {
        private IDocumentStore _ds;
        private bool _useCreationConverter = true;
        private readonly string _dataDirectory;
        private readonly bool _httpAccesss;
        private readonly Assembly[] _indexAssemblies;

        public bool UseCreationConverter
        {
            get { return _useCreationConverter; }
            set { _useCreationConverter = value; }
        }

        public RavenEmbeededDataStoreModule(string dataDirectory, bool httpAccesss = false, params Assembly[] indexAssemblies)
        {
            _dataDirectory = dataDirectory;
            _httpAccesss = httpAccesss;
            _indexAssemblies = indexAssemblies;
        }


        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterType<AutofacCreationConverter>().AsSelf().SingleInstance();

            builder.Register(context => InitDocumentStore(context.Resolve<AutofacCreationConverter>()))
                .AsSelf()
                .As<IDocumentStore>()
                .SingleInstance()
                .OnRelease(x=> { if (!x.WasDisposed) x.Dispose(); });

            builder
                .Register(context => context.Resolve<IDocumentStore>().OpenSession())
                .As<IDocumentSession>()
                .InstancePerDependency();
        }

        private IDocumentStore InitDocumentStore(AutofacCreationConverter converter)
        {
            IDocumentStore ds;
            
            if (_httpAccesss)
            {
                ds = new EmbeddableDocumentStore
                    {
                        DataDirectory = _dataDirectory,
                        UseEmbeddedHttpServer = true,
                        ResourceManagerId = Guid.NewGuid(),
                    };
                NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(9999);
            }
            else
            {
                ds = new EmbeddableDocumentStore()
                    {
                        DataDirectory = _dataDirectory,
                        ResourceManagerId = Guid.NewGuid()
                    };
            }
            if(converter!=null && UseCreationConverter)
            {
                ds.Conventions.CustomizeJsonSerializer += s => s.Converters.Add(converter);
            }           
            ds.Conventions.DisableProfiling = true;
            ds.Conventions.JsonContractResolver = new RecordClrTypeInJsonContractResolver();
            ds.Conventions.CustomizeJsonSerializer += s => s.TypeNameHandling = global::Raven.Imports.Newtonsoft.Json.TypeNameHandling.Arrays;
            
            ds.Initialize();

            global::Raven.Client.Indexes.IndexCreation.CreateIndexes(ThisAssembly, ds);
            if (_indexAssemblies != null)
            {
                foreach (var indexAssembly in _indexAssemblies)
                {
                    global::Raven.Client.Indexes.IndexCreation.CreateIndexes(indexAssembly, ds);                    
                }
            }

            //global::Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(RegionTrajectoryIndex).Assembly, ds);
            _ds = ds;
            return ds;
        }

        public bool CreateIndexes { get; set; }
    }
}