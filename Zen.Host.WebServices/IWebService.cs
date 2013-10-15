using System.ServiceModel;

namespace Zen.Host.WebServices
{
    [ServiceContract]
    public interface IWebService
    {
        [OperationContract]
        string GetWebserviceName();
    }
}