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

        public WebserviceHostApplication(IEnumerable<IWebService> knownServices,Config config)
        {
            _knownServices = knownServices;
            _config = config;
            _tokenSource = new CancellationTokenSource();
            _taskFactory = new TaskFactory(_tokenSource.Token);
        }

        public void Start()
        {
            foreach (var knownService in _knownServices)
            {
                IWebService service = knownService;
                var scope = AppScope.BeginScope(b => b.Register(ctx => this).As<WebserviceHostApplication>());
                _hostScopes.Add(scope);

                var hostName = _config.GetAppSettingString("Zen/Hostname");
                if (string.IsNullOrEmpty(hostName)) hostName = "localhost";

                var port = _config.GetAppSettingString("Zen/Port");
                if (string.IsNullOrEmpty(port)) port = "8080";

                _taskFactory.StartNew(() =>
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
                    });
            }
        }

        private readonly ILog Log = LogManager.GetLogger(typeof (WebserviceHostApplication));
        public void Stop()
        {
            foreach (var host in _hosts)
            {
                host.Close(TimeSpan.FromMinutes(1));
            }
            _tokenSource.Cancel();

            foreach (var hostScope in _hostScopes)
            {
                hostScope.Dispose();
            }
        }

        public AppScope AppScope { get; set; }
    }
}