using Autofac;

namespace Zen.DataStore.Mongo
{
    public class MongoDataStoreModule:Module
    {
        private readonly string _connectionString;
        private readonly string _databaseName;

        public MongoDataStoreModule(string connectionString = "mongodb://localhost", string databaseName = "ZenDatabase")
        {
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(ctx => new MongoDbContext(_connectionString, _databaseName))
                   .AsSelf().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof (BasicMongoRepository<>))
                   .As(typeof (IRepository<>));
            builder.RegisterGeneric(typeof(BasicMongoRepositoryWithGuid<>))
                   .As(typeof(IRepositoryWithGuid<>));
        }
    }
}