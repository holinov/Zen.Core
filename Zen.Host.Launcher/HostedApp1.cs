using System;
using System.Threading;
using log4net;

namespace Zen.Host.Launcher
{
    public class HostedApp1 : IHostedApp
    {
        private bool _runing = false;
        private DispObject _dispObject;
        private readonly DispObject _dispObject1;
        private static readonly ILog Log = LogManager.GetLogger(typeof (HostedApp1));

        public HostedApp1(DispObject dispObject, DispObject dispObject1)
        {
            _dispObject = dispObject;
            _dispObject1 = dispObject1;
        }

        public void Start()
        {
            _runing = true;
            Log.Debug("Запуск другого приложения");
            //while (_runing)
            //{
                Thread.Sleep(1000);
            throw new ApplicationException("А тут мы кинем ошибку");
            //}
            Log.Info("stopped 1 by self");
        }

        public void Stop()
        {
            //_runing = false;
            //Log.Debug("stop 1");
        }
    }
}