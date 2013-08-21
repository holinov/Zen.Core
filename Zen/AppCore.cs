using Autofac;

namespace Zen
{
    public class AppCore : AppScope
    {
        public AppCore(IContainer container)
            : base(container)
        {

        }
    }
}