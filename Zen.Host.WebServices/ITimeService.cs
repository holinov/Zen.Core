using System;
using System.ServiceModel;

namespace Zen.Host.WebServices
{
    [ServiceContract]
    public interface ITimeService : IWebService
    {
        [OperationContract]
        DateTime GetSystemTime();

        [OperationContract]
        DateTime GetSystemTimeUtc();
    }
}