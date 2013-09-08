using Autofac;

namespace Zen.DataStore.Raven
{
    /// <summary>
    ///     Модуль стандартных репозиториев Raven
    /// </summary>
    public class RavenRepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof (BasicRavenRepository<>))
                   .As(typeof (IRepository<>));

            builder.RegisterGeneric(typeof (BasicRavenRepositoryWithGuid<>))
                //.As(typeof(IRepository<>))
                   .As(typeof (IRepositoryWithGuid<>));

            builder.RegisterGeneric(typeof (Refrence<>))
                   .PropertiesAutowired()
                   .AsSelf();
/*
            builder.RegisterGeneric(typeof(EventsRepository<>))
                .As(typeof(IEventsRepository<>));

            builder.RegisterType<UserProfileRavenRepository>()
                .As<IUserProfileRepository>();

            builder.RegisterType<ObjectStrategyRepository>()
                .As<IObjectStrategyRepository>();

            builder.RegisterType<ReportRepository>()
                .As<IReportRepository>();

            builder.RegisterType<RegionStrategyRepository>()
                .As<IRegionStrategyRepository>();
            
            builder.RegisterType<SensorsStrategyRepository>()
                            .As<ISesnorsStrategyRepository>();

            builder.RegisterType<ObjectsRepository>()
                .As<IObjectsRepository>();

            builder.RegisterType<CategoryRepository>()
                .As<ICategoryRepository>();
            
            builder.RegisterType<TicketsRepository>()
                .As<ITicketsRepository>();

            builder.RegisterType<DCUStartTimesRepository>().As<IDCUStartTimesRepository>();

            builder.RegisterType<SensorsRepository>()
                .As<ISensorsRepository>();*/


            /* builder.RegisterAssemblyTypes(typeof(TrajectoryCoord).Assembly)
                .AssignableTo(typeof(IEventStreamStrategy<>))
                .AsSelf()
                .AsImplementedInterfaces();*/
        }
    }
}