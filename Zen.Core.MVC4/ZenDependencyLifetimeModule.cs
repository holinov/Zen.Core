using System;
using System.Collections.Generic;
using System.Web;
using Zen.Core.MVC4;

[assembly: PreApplicationStartMethod(typeof(ZenModulesInit), "Initialize")]

namespace Zen.Core.MVC4
{
    public class ZenDependencyLifetimeModule : IHttpModule
    {
        //private AppScope _scope;
        private static readonly Dictionary<HttpRequest, AppScope> Scopes=new Dictionary<HttpRequest, AppScope>();

        public static AppCore Core { get; set; }


        public void Init(HttpApplication context)
        {
            //ZenMvcDependencyScopeResolver.AppScope = _scope;
            context.BeginRequest += context_BeginRequest;
            context.EndRequest += context_EndRequest;
        }

        void context_EndRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            HttpRequest req = context.Request;
            lock (req)
            {
                if (Scopes.ContainsKey(req))
                {
                    Scopes[req].Dispose();
                    Scopes.Remove(req);
                }
            }
        }

        private void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication) sender;
            HttpContext context = application.Context;
            HttpRequest req = context.Request;
            lock (req)
            {
                var scope = Core.BeginScope();
                Scopes[req] = scope;
                ZenMvcDependencyScopeResolver.AppScope = scope;
            }
        }

        public void Dispose()
        {
            //_scope.Dispose();
            
        }
    }
}