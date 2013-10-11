using System.Configuration;
using System.ServiceProcess;
using Autofac;
using log4net.Config;

namespace Zen.Host.Launcher
{
    partial class ServiceAppHost : ServiceBase
    {
        private readonly AppHost _host;

        public ServiceAppHost()
        {
            InitializeComponent();
            XmlConfigurator.Configure();
            var coreBuilder = HostConfigurator.GetBuilder();

            var core = coreBuilder.Build();

            _host = new AppHost(core);
        }

        protected override void OnStart(string[] args)
        {
            _host.Start();
        }

        protected override void OnStop()
        {
            _host.Stop();
        }
    }
}
