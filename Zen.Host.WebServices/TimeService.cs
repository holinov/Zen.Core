using System;

namespace Zen.Host.WebServices
{
    public class TimeService : ITimeService
    {
        public DateTime GetSystemTime()
        {
            return DateTime.Now;
        }

        public DateTime GetSystemTimeUtc()
        {
            return DateTime.UtcNow;
        }

        public string GetWebserviceName()
        {
            return typeof (TimeService).Name;
        }
    }
}