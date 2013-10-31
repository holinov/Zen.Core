using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Zen;
using Zen.Core.MVC4;

namespace ZenMvc4Test
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static AppCore _core;

        /*static MvcApplication()
        {
            DynamicModuleUtility.RegisterModule(typeof(ZenDependencyLifetimeModule));
        }*/
        protected void Application_Start()
        {

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
           
            //DependencyResolver.SetResolver(_core.Resolve<IDependencyResolver>());
            _core = AppCoreBuilder.Create()
                                  .Configure(b => b.RegisterType<Dep>().AsSelf())
                                  .ConfigureMvcResolver()
                                  .RegisterControllers(typeof (MvcApplication).Assembly)
                                  .Build()
                                  .SetupResolvers();

        }

        protected void Application_End()
        {
            _core.Dispose();
        }
    }

    public class Dep
    {

    }    
}