using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.Wcf;
using log4net;

namespace Zen.Host.WebServices
{
    public class WebserviceHostApplication : IHostedApp
    {
        private readonly IEnumerable<IWebService> _knownServices;
        private readonly Config _config;
        private readonly CancellationTokenSource _tokenSource;
        private readonly TaskFactory _taskFactory;
        private readonly List<ServiceHost> _hosts = new List<ServiceHost>();
        private readonly List<IAppScope> _hostScopes = new List<IAppScope>();
        private readonly List<Thread> _hostThreads = new List<Thread>(); 

        public WebserviceHostApplication(IEnumerable<IWebService> knownServices,Config config)
        {
            _knownServices = knownServices;
            _config = config;
            _tokenSource = new CancellationTokenSource();
            _taskFactory = new TaskFactory(_tokenSource.Token);
        }

        public void Start()
        {
            var services = _knownServices.ToArray();
            Log.InfoFormat("Запуск WCF-хоста для {0} сервисов", services.Length);
            var hostName = _config.GetAppSettingString("Zen/Hostname");
            if (string.IsNullOrEmpty(hostName)) hostName = "localhost";

            var port = _config.GetAppSettingString("Zen/Port");
            if (string.IsNullOrEmpty(port)) port = "8080"; 
            
            foreach (var knownService in services)
            {
                Log.DebugFormat("Запуск хостов для типа: {0}", knownService.GetType().Name);
                IWebService service = knownService;
                var scope = AppScope.BeginScope(b => b.Register(ctx => this).As<WebserviceHostApplication>());
                _hostScopes.Add(scope);
                _hostThreads.Add(new Thread(() =>
                    {
                        var uriStr = string.Format("http://{0}:{1}/{2}", hostName, port, service.GetWebserviceName());
                        Uri address = new Uri(uriStr);

                        foreach (
                            var webService in
                                service.GetType()
                                       .GetInterfaces()
                                       .Where(i =>
                                              i.GetCustomAttributes(typeof (ServiceContractAttribute), true).Any() 
                                              && i != typeof (IWebService)))
                        {
                            try
                            {
                                Log.DebugFormat("Запуск хостов для типа: {0} по контракту {1}", service.GetType().Name,
                                                webService.Name);
                                ServiceHost host = new ServiceHost(service.GetType(), address);
                                host.AddServiceEndpoint(webService, new BasicHttpBinding(), string.Empty);

                                host.AddDependencyInjectionBehavior(webService, scope.Scope);

                                host.Description.Behaviors.Add(new ServiceMetadataBehavior
                                    {
                                        HttpGetEnabled = true,
                                        HttpGetUrl = address
                                    });
                                _hosts.Add(host);
                                host.Open();
                                var sb = new StringBuilder();
                                foreach (var ep in host.Description.Endpoints)
                                {
                                    sb.AppendFormat(
                                        "Для контракта: {1} сервиса {2} производится прослушивание по адресу {0}",
                                        ep.Address.Uri.AbsoluteUri,
                                        ep.Contract.Name,
                                        service.GetType().Name)
                                      .AppendLine();
                                }
                                Log.Info(sb.ToString());

                                var enumService = scope.Resolve<EnumeratorWebservice>();
                                if (enumService != null) enumService.RegisterService(service);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(string.Format("Ошибка инициализации веб-сервиса {1}:{0}", webService.Name,
                                                        service.GetType().Name), ex);
                            }
                        }
                    }));
            }

            foreach (var hostThread in _hostThreads)
            {
                hostThread.Start();
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof (WebserviceHostApplication));
        public void Stop()
        {
            foreach (var host in _hosts)
            {
                try
                {
                    host.Close(TimeSpan.FromMinutes(1));
                }
                catch (Exception ex)
                {
                    Log.Warn("Ошибка остановки хоста веб-сервиса", ex);
                }
            }
            _tokenSource.Cancel();

            foreach (var thread in _hostThreads)
            {
                thread.Abort();
            }

            foreach (var hostScope in _hostScopes)
            {
                hostScope.Dispose();
            }
        }

        public IAppScope AppScope { get; set; }
    }
}