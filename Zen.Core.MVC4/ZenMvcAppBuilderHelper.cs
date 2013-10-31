using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using IDependencyResolver = System.Web.Mvc.IDependencyResolver;

namespace Zen.Core.MVC4
{
    public static class ZenMvcAppBuilderHelper
    {
        /// <summary>
        /// Зарегистрировать контроллер в ядре
        /// </summary>
        /// <typeparam name="TController">Контроллер</typeparam>
        /// <param name="builder">Построитель ядра</param>
        /// <returns>Построитель ядра</returns>
        public static AppCoreBuilder RegisterController<TController>(this AppCoreBuilder builder)
        {
            builder.Configure(
                b =>
                b.RegisterType<TController>()
                 .AsSelf()
                 .As<Controller>()
                 .InstancePerLifetimeScope());
            return builder;
        }
        /// <summary>
        /// Зарегистрировать контроллеры из сборки
        /// </summary>
        /// <param name="builder">Построитель ядра</param>
        /// <param name="controllerAssemblies">Сборки из которых загружать контроллеры</param>
        /// <returns></returns>
        public static AppCoreBuilder RegisterControllers(this AppCoreBuilder builder, params Assembly[] controllerAssemblies)
        {

            builder.Configure(
                b =>
                b.RegisterAssemblyTypes(controllerAssemblies)
                 .AssignableTo<Controller>()
                 .AsSelf()
                 .As<Controller>()
                 .InstancePerLifetimeScope());

            return builder;
        }

        /// <summary>
        /// Зарегистрировать IDependencyResolver
        /// </summary>
        /// <param name="builder">Построитель ядра</param>
        /// <returns>Построитель ядра</returns>
        public static AppCoreBuilder ConfigureMvcResolver(this AppCoreBuilder builder)
        {
            builder.Configure(b => b.RegisterType<ZenMvcDependencyResolver>().As<IDependencyResolver>());
            return builder;
        }

        /// <summary>
        /// Установить IoC резолверы по данным ядра
        /// </summary>
        /// <param name="core">Ядро</param>
        /// <returns>Ядро</returns>
        public static AppCore SetupResolvers(this AppCore core)
        {
            ZenDependencyLifetimeModule.Core = core;
            ZenMvcDependencyResolver.Core = core;
            DependencyResolver.SetResolver(core.Resolve<IDependencyResolver>());
            return core;
        }
        
    }
}
