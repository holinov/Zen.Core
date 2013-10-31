using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace Zen.Core.MVC4
{
    public static class ZenModulesInit
    {
        public static void Initialize()
        {
            DynamicModuleUtility.RegisterModule(typeof(ZenDependencyLifetimeModule));
        }
    }
}