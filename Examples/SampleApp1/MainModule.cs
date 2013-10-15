using Autofac;
using Zen.DataStore.Raven;
using Zen.DataStore.Raven.Embeeded;

namespace SampleApp1
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //Регистрация модуля подключения к RavenDB Embeeded
            builder.RegisterModule(new RavenEmbeededDataStoreModule("DataDirectory", true));
            //Регистрация модуля репозитариев RavenDB
            builder.RegisterModule<RavenRepositoriesModule>();

            //Регистрация вебсервисов
            builder.RegisterType<HelloWorldService>()
                   .AsImplementedInterfaces()
                   .AsSelf();
        }
    }
}