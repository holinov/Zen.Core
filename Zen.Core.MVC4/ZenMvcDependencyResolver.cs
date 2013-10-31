using System.Web.Mvc;

namespace Zen.Core.MVC4
{
    public class ZenMvcDependencyResolver : ZenMvcDependencyScopeResolver, IDependencyResolver
    {
        public ZenMvcDependencyResolver(AppCore core)
            : base(core)
        {
        }

        public override void Dispose()
        {
            //Не разрушаем ядро
        }
    }
}