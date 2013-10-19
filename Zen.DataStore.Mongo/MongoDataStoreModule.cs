using Autofac;

namespace Zen.DataStore.Mongo
{
    public class MongoDataStoreModule:Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MongoDbContext>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof (BasicMongoRepository<>))
                   .As(typeof (IRepository<>));
            builder.RegisterGeneric(typeof(BasicMongoRepositoryWithGuid<>))
                   .As(typeof(IRepositoryWithGuid<>));
        }
    }
}