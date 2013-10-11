using System.Configuration;

namespace Zen.Host.Launcher
{
    public class HostConfig : ConfigurationSection
    {
        [ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public HostedAppCollection HostedApps
        {
            get { return (HostedAppCollection) base[""]; }
            set { base[""] = value; }
        }

        [ConfigurationProperty("ScanAll", IsRequired = true)]
        public bool ScanAll
        {
            get
            {
                return (bool)this["ScanAll"];
            }
            set
            {
                this["ScanAll"] = value;
            }
        }
    }
}