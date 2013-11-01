using System;
using System.Reflection;
using Autofac;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Database.Server;
using Raven.Imports.Newtonsoft.Json;
using Module = Autofac.Module;

namespace Zen.DataStore.Raven.Embeeded
{
    /// <summary>
    ///     Регистрация DataStorage и фабрики сессий
    /// </summary>
    public class RavenEmbeededDataStoreModule : Module
    {
        private readonly string _dataDirectory;
        private readonly bool _httpAccesss;
        private readonly Assembly[] _indexAssemblies;
        private bool _useCreationConverter = true;
        private readonly int _httpAccesssPort;

        public RavenEmbeededDataStoreModule(string dataDirectory, bool httpAccesss = false, int httpAccesssPort = 9999,
                                            params Assembly[] indexAssemblies)
        {
            _dataDirectory = dataDirectory;
            _httpAccesss = httpAccesss;
            _httpAccesssPort = httpAccesssPort;
            _indexAssemblies = indexAssemblies;
        }

        public bool UseCreationConverter
        {
            get { return _useCreationConverter; }
            set { _useCreationConverter = value; }
        }


        public bool CreateIndexes { get; set; }

        public bool RunInMemory { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(ctx=>new AutofacCreationConverter(AppCore.Instance)).AsSelf().SingleInstance();

            builder.Register(context => InitDocumentStore(context.Resolve<AutofacCreationConverter>()))
                   .AsSelf()
                   .As<IDocumentStore>()
                   .SingleInstance()
                   .OnRelease(x => { if (!x.WasDisposed) x.Dispose(); });           

            builder
                .Register(context => context.Resolve<IDocumentStore>().OpenSession())
                .As<IDocumentSession>()
                .InstancePerLifetimeScope();
        }

        private IDocumentStore InitDocumentStore(AutofacCreationConverter converter)
        {
            var ds = new EmbeddableDocumentStore
                {
                    DataDirectory = _dataDirectory,
                    ResourceManagerId = Guid.NewGuid(),
                    RunInMemory = RunInMemory,
                };

            if (_httpAccesss)
            {
                ds.UseEmbeddedHttpServer = true;
                NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(_httpAccesssPort);
            }

            if (converter != null && UseCreationConverter)
            {
                ds.Conventions.CustomizeJsonSerializer += s => s.Converters.Add(converter);
            }
            ds.Conventions.DisableProfiling = true;
            ds.Conventions.JsonContractResolver = new RecordClrTypeInJsonContractResolver();
            ds.Conventions.CustomizeJsonSerializer += s => s.TypeNameHandling = TypeNameHandling.Arrays;

            ds.Initialize();

            IndexCreation.CreateIndexes(ThisAssembly, ds);
            if (_indexAssemblies != null)
            {
                foreach (var indexAssembly in _indexAssemblies)
                {
                    IndexCreation.CreateIndexes(indexAssembly, ds);
                }
            }

            //global::Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(RegionTrajectoryIndex).Assembly, ds);
            return ds;
        }
    }
}