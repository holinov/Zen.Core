using System;
using System.Configuration;
using System.Reflection;
using Autofac;
using log4net;

namespace Zen.Host.Launcher
{
    public static class HostConfigurator
    {
        private static AppCoreBuilder _coreBuilder;
        private static readonly ILog Log = LogManager.GetLogger(typeof (HostConfigurator));

        public static AppCoreBuilder GetBuilder()
        {
            if (_coreBuilder == null)
            {
                _coreBuilder = AppCoreBuilder.Create()
                                             /*.Configure(b => b.RegisterAssemblyTypes(typeof (Program).Assembly)
                                                              .AssignableTo<IHostedApp>()
                                                              .AsImplementedInterfaces()
                                                              .AsSelf())*/
                                             .Configure(b => b.RegisterType<DispObject>().AsSelf().InstancePerLifetimeScope());
            }
            var cfg = ConfigurationManager.GetSection("HostConfig") as HostConfig??new HostConfig();
            _coreBuilder.Configure(b => b.Register(ctx => cfg).As<HostConfig>().SingleInstance());
            if (!cfg.ScanAll)
            {
                Log.Debug("Загрузка приложений из списка");
                foreach (HostedAppElement hostedApp in cfg.HostedApps)
                {
                    LoadHostedApps(hostedApp.HostedAssembly, hostedApp.LoadModules, _coreBuilder);
                }
            }
            else
            {
                Log.Debug("Загрузка всех приложений из сборок загруженных в домен");
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    LoadHostedApps(assembly,true,_coreBuilder);
                }
            }
            return _coreBuilder;
        }

        public static void LoadHostedApps(Assembly hostedAssembly, bool loadModules, AppCoreBuilder coreBuilder)
        {
            Log.DebugFormat("Сканирование сборки {0}. Загружать модули: {1}", hostedAssembly.FullName, loadModules);
            if (loadModules)
                coreBuilder.Configure(b => b.RegisterAssemblyModules(hostedAssembly));

            coreBuilder.Configure(b => b.RegisterAssemblyTypes(hostedAssembly)
                                        .AssignableTo<IHostedApp>()
                                        .AsImplementedInterfaces()
                                        .AsSelf());
        }

        public static void LoadHostedApps(string hostedAssembly, bool loadModules, AppCoreBuilder coreBuilder)
        {
            Log.DebugFormat("Загрузка сборки {0} по имени",hostedAssembly);
            try
            {
                var loadedAssembly = Assembly.Load(new AssemblyName(hostedAssembly));
                LoadHostedApps(loadedAssembly, loadModules, coreBuilder);
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка загрузки сборки приложения " + hostedAssembly, ex);
            }
        }
    }
}