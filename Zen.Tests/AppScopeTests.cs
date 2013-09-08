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
                }
            }
        }
    }
}