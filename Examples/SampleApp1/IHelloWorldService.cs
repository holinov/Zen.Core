using System.ServiceModel;
using Zen.Host.WebServices;

namespace SampleApp1
{
    [ServiceContract]
    public interface IHelloWorldService : IWebService
    {
        [OperationContract]
        string Hello(string name);
    }
}