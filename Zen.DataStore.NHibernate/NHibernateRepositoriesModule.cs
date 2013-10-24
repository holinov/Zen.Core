using Autofac;

namespace Zen.DataStore.NHibernate
{
    public class NHibernateRepositoriesModule:Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof (BasicNHibernateRepository<>))
                   .As(typeof (IRepository<>));

            builder.RegisterGeneric(typeof(BasicNHibernateRepositoryWithGuid<>))
                   .As(typeof(IRepositoryWithGuid<>));
        }
    }
}