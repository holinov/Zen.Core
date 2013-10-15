using System.ComponentModel;

namespace Zen.Host.Launcher
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private static string _serviceName="Zen.Host.Service";

        public ProjectInstaller()
        {
            InitializeComponent();
            serviceInstaller1.ServiceName = ServiceName;
        }

        public static string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
    }
}
