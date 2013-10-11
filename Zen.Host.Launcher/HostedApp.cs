using System.Threading;
using log4net;

namespace Zen.Host.Launcher
{
    public class HostedApp : IHostedApp
    {
        private bool _runing = false;
        private DispObject _dispObject;
        private readonly DispObject _dispObject1;
        private static readonly ILog Log = LogManager.GetLogger(typeof(HostedApp));

        public HostedApp(DispObject dispObject, DispObject dispObject1)
        {
            _dispObject = dispObject;
            _dispObject1 = dispObject1;
        }

        public void Start()
        {
            _runing = true;
            Log.Debug("Запуск приложения");
            while (_runing)
            {
                Thread.Sleep(100);
            }
            Log.Info("Отработало до конца");
        }

        public void Stop()
        {
            _runing = false;
            Log.Debug("Остановка");
        }
    }
}