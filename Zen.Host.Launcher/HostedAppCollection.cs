using System.Configuration;

namespace Zen.Host.Launcher
{
    public class HostedAppCollection : ConfigurationElementCollection
    {
        const string ELEMENT_NAME = "HostedApp";

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return ELEMENT_NAME; }
        }
        
        protected override ConfigurationElement CreateNewElement()
        {
            return new HostedAppElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var e = element as HostedAppElement;
            if (e != null)
                return e.HostedAssembly;
            else return string.Empty;
        }
    }
}