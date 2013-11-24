using System;
using System.Configuration;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using Zen.Host.WebServices;
using log4net;
using log4net.Config;

namespace Zen.Host.Launcher
{
    static class Program
    {
        static void Main(string[] args)
        {

            XmlConfigurator.Configure();
            if (args.Length > 0)
            {
                //Установка или удаление сервиса
                var action = args[0];
                RunServiceInstaller(action);
            }
            else
            {
                if (Environment.UserInteractive)
                {
                    //Запуск в режиме консольного приложения
                    RunConsole();
                }
                else
                {
                    ServiceBase.Run(new ServiceAppHost());
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        private static void RunServiceInstaller(string action)
        {
            switch (action)
            {
                case "-reinstall":
                case "-r":
                    {
                        try
                        {
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location }); 
                        }
                        catch (Exception ex)
                        {
                            Log.Fatal("Ошибка удаления сервиса", ex);
                        }
                        try
                        {
                            ManagedInstallerClass.InstallHelper(new string[] {Assembly.GetExecutingAssembly().Location});
                        }
                        catch (Exception ex)
                        {
                            Log.Fatal("Ошибка установки сервиса", ex);
                        }
                        break;
                    }
                case "-install":
                case "-i":
                    {
                        try
                        {
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                        }
                        catch (Exception ex)
                        {
                            Log.Fatal("Ошибка установки сервиса", ex);
                        }
                        break;
                    }
                case "-uninstall":
                case "-u":
                    {
                        try
                        {
                            ManagedInstallerClass.InstallHelper(new string[]
                                {"/u", Assembly.GetExecutingAssembly().Location});
                        }
                        catch (Exception ex)
                        {
                            Log.Fatal("Ошибка удаления сервиса", ex);                            
                        }
                        break;
                    }
            }
            Console.WriteLine("Нажмите ENTER для выхода");
            Console.ReadLine();
        }

        private static void RunConsole()
        {
            var coreBuilder = HostConfigurator.GetBuilder();
            //HostConfigurator.LoadHostedApps(typeof(Program).Assembly, false, coreBuilder);
            //HostConfigurator.LoadHostedApps(typeof(IWebService).Assembly, true, coreBuilder);

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
