using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Zen;
using Zen.Host;

namespace ZenHostTest
{
    public class TestModule:Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TestApp>().AsImplementedInterfaces().AsSelf();
            //base.Load(builder);
        }
    }
    public class TestApp : IHostedApp
    {
        private readonly IAppScope _scope;

        public TestApp(IAppScope scope)
        {
            _scope = scope;
        }

        public void Start()
        {
            //
        }

        public void Stop()
        {
            //
        }

        public AppScope AppScope { get; set; }
    }
}
