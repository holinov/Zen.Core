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
            builder.RegisterType<Dep>().AsSelf();
            //base.Load(builder);
        }
    }

    public class Dep
    {
        private readonly IAppScope _scope;

        public Dep(IAppScope scope)
        {
            _scope = scope;
        }
    }
    public class TestApp : IHostedApp
    {
        private readonly IAppScope _scope;
        private readonly Dep _dep;

        public TestApp(IAppScope scope,Dep dep)
        {
            _scope = scope;
            _dep = dep;
        }

        public void Start()
        {
            //
        }

        public void Stop()
        {
            //
        }

        public IAppScope AppScope { get; set; }
    }
}
