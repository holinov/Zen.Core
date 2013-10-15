using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zen.Host.WebServices
{
    public class EnumeratorWebservice : IEnumeratorWebservice
    {
        public string GetWebserviceName()
        {
            return typeof (EnumeratorWebservice).Name;
        }

        private readonly List<string> _services=new List<string>(); 

        public string[] GetKnownServices()
        {
            return _services.ToArray();
        }

        public void RegisterService(IWebService service)
        {
            _services.Add(service.GetWebserviceName());
            _services.Sort();
        }
    }
}
