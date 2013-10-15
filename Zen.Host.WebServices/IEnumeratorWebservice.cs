using System.ServiceModel;

namespace Zen.Host.WebServices
{
    [ServiceContract]
    public interface IEnumeratorWebservice : IWebService
    {
        /// <summary>
        /// Получить имена известных сервисов
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string[] GetKnownServices();
    }
}