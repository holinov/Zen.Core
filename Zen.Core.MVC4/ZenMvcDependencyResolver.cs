using System.Web.Mvc;

namespace Zen.Core.MVC4
{
    public class ZenMvcDependencyResolver : ZenMvcDependencyScopeResolver, IDependencyResolver
    {
        public ZenMvcDependencyResolver(AppCore core):base(core)
        {
            ZenMvcDependencyResolver.Core = core;
        }

        public static AppCore Core { get; set; }

        public override void Dispose()
        {
            //Не разрушаем ядро
        }
    }
}