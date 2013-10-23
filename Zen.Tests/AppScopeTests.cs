using Autofac;
using NUnit.Framework;

namespace Zen.Tests
{
    [TestFixture]
    public class AppScopeTests
    {
        public class TestClass1
        {
            public int Val { get; set; }
        }

        public class TestClass2
        {
            public int Val { get; set; }
        }


        public interface IInterface
        {
            TestClass1 Test1 { get; }
            TestClass2 Test2 { get; } 
        }

        [Test]
        public void EmitInterfaceImplementorTest()
        {
            EmitInterfaceImplementorBase.SaveCache = false;
            var builder = new ContainerBuilder();
            builder.RegisterType<TestClass1>().AsSelf().InstancePerDependency();
            builder.RegisterType<TestClass2>().AsSelf().InstancePerLifetimeScope();

            using (var core = AppCoreBuilder
                .Create(builder)
                .AddModule<EmitImplementerModule>()
                .Configure(b => b.RegisterInterfaceForEmit<IInterface>())
                .Build())
            {
                using (var scope = core.BeginScope())
                {
                    var vaf = scope.Resolve<EmitInterfaceImplementor<IInterface>>().ImplementInterface();
                    Assert.NotNull(vaf);
                    Assert.NotNull(vaf.Test1);
                    Assert.NotNull(vaf.Test2);

                    var vaf1 = scope.Resolve<IInterface>();
                    Assert.AreNotSame(vaf, vaf1);
                    Assert.AreSame(vaf.Test2, vaf1.Test2);
                    Assert.AreNotSame(vaf.Test1, vaf1.Test1);
                }
            }
        }

        [Test]
        public void AppScopeTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestClass1>().AsSelf().SingleInstance();
            builder.RegisterType<TestClass2>().AsSelf().InstancePerLifetimeScope();
            IContainer cont = builder.Build();

            using (var scope = new AppScope(cont))
            {
                object r1 = scope.Resolve(typeof (TestClass1));
                var r2 = scope.Resolve<TestClass1>();
                Assert.AreSame(r1, r2);
                Assert.AreSame(cont, scope.Scope);

                var t1 = scope.Resolve<TestClass2>();
                using (AppScope inner = scope.BeginScope())
                {
                    var t2 = inner.Resolve<TestClass2>();
                    Assert.AreNotSame(t1, t2);

                    var resolvedScope = inner.Resolve<IAppScope>();
                    Assert.AreSame(inner,resolvedScope);
                }
            }
        }
    }
}