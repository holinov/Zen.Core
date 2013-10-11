using System;
using System.Configuration;
using System.Configuration.Install;
using System.Reflection;
using log4net.Config;

namespace Zen.Host.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                //Установка или удаление сервиса
                var action = args[0];
                RunServiceInstaller(action);
            }
            else
            {
                //Запуск в режиме консольного приложения
                RunConsole();                
            }
        }

        private static void RunServiceInstaller(string action)
        {
            switch (action)
            {
                case "-install":
                case "-i":
                    {
                        ManagedInstallerClass.InstallHelper(new string[] {Assembly.GetExecutingAssembly().Location});
                        break;
                    }
                case "-uninstall":
                case "-u":
                    {
                        ManagedInstallerClass.InstallHelper(new string[] {"/u", Assembly.GetExecutingAssembly().Location});
                        break;
                    }
            }
        }

        private static void RunConsole()
        {
            XmlConfigurator.Configure();
            var coreBuilder = HostConfigurator.GetBuilder();
            HostConfigurator.LoadHostedApps(typeof (Program).Assembly, false, coreBuilder);

            var core = coreBuilder.Build();
            var host = new AppHost(core);
            host.Start();

            Console.WriteLine("Any key to stop");
            Console.ReadKey();

            host.Stop();

            Console.WriteLine("Any key to exit");
            Console.ReadKey();
        }
    }
}
