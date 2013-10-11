using System;
using System.Configuration;

namespace Zen.Host.Launcher
{
    public class HostedAppElement : ConfigurationElement
    {
        [ConfigurationProperty("HostedAssembly", IsRequired = true)]
        public String HostedAssembly
        {
            get
            {
                return (String)this["HostedAssembly"];
            }
            set
            {
                this["HostedAssembly"] = value;
            }
        }

        [ConfigurationProperty("LoadModules", DefaultValue = true)]
        public bool LoadModules
        {
            get
            {
                return (bool)this["LoadModules"];
            }
            set
            {
                this["LoadModules"] = value;
            }
        }
    }
}