using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autofac;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Shard;

namespace Zen.DataStore.Raven
{
    /*
    /// <summary>
    ///     Регистрация DataStorage и фабрики сессий
    /// </summary>
    public class RavenSegmentedDataStoreModule : Module, IShardResolutionStrategy
    {
        private readonly Dictionary<string, IDocumentStore> _shards;

        public RavenSegmentedDataStoreModule(Dictionary<string, IDocumentStore> shards)
        {
            _shards = shards;
        }

        public RavenSegmentedDataStoreModule()
            : this(new Dictionary<string, IDocumentStore>
                {
                    {"defaultShard", new DocumentStore {Url = "http://localhost:9901"}},
                    {"reg-1.0", new DocumentStore {Url = "http://localhost:9901"}},
                    {"reg-2.0", new DocumentStore {Url = "http://localhost:9902"}},
                })
        {
        }

        /// <summary>
        ///     Generate a shard id for the specified entity
        /// </summary>
        public string GenerateShardIdFor(object entity, ITransactionalDocumentSession sessionMetadata)
        {
            return GenerateShardIdFor(entity);
        }

        /// <summary>
        ///     The shard id for the server that contains the metadata (such as the HiLo documents)
        ///     for the given entity
        /// </summary>
        public string MetadataShardIdFor(object entity)
        {
            return _shards.First().Key;
        }

        /// <summary>
        ///     Selects the shard ids appropriate for the specified data.
        /// </summary>
        /// <returns>
        ///     Return a list of shards ids that will be search. Returning null means search all shards.
        /// </returns>
        public IList<string> PotentialShardsFor(ShardRequestData requestData)
        {
            if (requestData.Query != null)
            {
                var shardIdRxpr =
                    new Regex(
                        string.Format(
                            "\r\n{0}: \\s* (?<Open>\")(?<shardId>[^\"]+)(?<Close-Open>\") |\r\n{0}: \\s* (?<shardId>[^\"][^\\s]*)",
                            Regex.Escape("SegmentId")), RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
                Match match = shardIdRxpr.Match(requestData.Query.Query);
                if (match.Success)
                {
                    string shardId = match.Groups["shardId"].ToString();
                    var res = new List<string>();
                    if (_shards.Keys.Any(k => k.StartsWith(shardId)))
                    {
                        res.Add(shardId);
                    }
                    if (res.Count > 0)
                        return res;
                }
            }
            return null;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutofacCreationConverter>().AsSelf().SingleInstance();

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
            var shardStrategy = new ShardStrategy(_shards)
                {
                    ShardAccessStrategy = new ParallelShardAccessStrategy(),
                    ShardResolutionStrategy = this
                };

            var ds = new ShardedDocumentStore(shardStrategy);
            ds.Initialize();
            if (converter != null)
            {
                ds.Conventions.CustomizeJsonSerializer += s => s.Converters.Add(converter);
            }
            return ds;
        }

        /// <summary>
        ///     Generate a shard id for the specified entity
        /// </summary>
        public string GenerateShardIdFor(object entity)
        {
            string shardId = "defaultShard";
            var isharded = entity as IHasSegmentId;
            if (isharded != null)
            {
                string id = isharded.SegmentId;
                if (_shards.ContainsKey(id))
                    shardId = id;
            }
            return shardId;
        }
    }*/
}