using System;
using log4net;

namespace Zen.Host.Launcher
{
    public class DispObject:IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DispObject));
        public void Dispose()
        {
            Log.Debug("Разрушение объекта DispObject в области видимости приложения");
        }
    }
}