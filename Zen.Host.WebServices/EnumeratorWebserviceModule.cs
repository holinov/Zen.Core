using Autofac;

namespace Zen.Host.WebServices
{
    public class EnumeratorWebserviceModule:Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EnumeratorWebservice>()
                   .AsImplementedInterfaces()
                   .AsSelf()
                   .SingleInstance();


            builder.RegisterType<TimeService>()
                   .AsImplementedInterfaces()
                   .AsSelf();
        }
    }
}