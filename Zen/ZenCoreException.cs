using System;

namespace Zen
{
    public class ZenCoreException : Exception
    {
        public ZenCoreException(string message) : base(message)
        {
            
        }
        public ZenCoreException(string message, Exception inner):base(message,inner)
        {
            
        }
    }
}