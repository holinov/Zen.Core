using Autofac;
using NUnit.Framework;

namespace Zen.Tests
{
    [TestFixture]
    public class AppCoreBuilderTests
    {
        public class TestClass1
        {
            public int Val { get; set; }
        }

        public class TestClass2
        {
            public int Val { get; set; }
        }

        public class TestClass3
        {
            public int Val { get; set; }
        }

        public class TestModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<TestClass2>().InstancePerDependency();
            }
        }

        public class TestModule1 : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<TestClass3>().InstancePerDependency();
            }
        }

        [Test]
        public void AppCoreBuilderTest()
        {
            AppCoreBuilder.Create(new ContainerBuilder()).Build().Dispose();

            using (
                AppCore core =
                    AppCoreBuilder.Create()
                                  .AddModule(new TestModule())
                                  .AddModule<TestModule1>()
                                  .Configure(b => b.RegisterType<TestClass1>().AsSelf().SingleInstance())
                                  .Build())
            {
                using (AppScope scope = core.BeginScope())
                {
                    object r1 = scope.Resolve(typeof (TestClass1));
                    var r2 = scope.Resolve<TestClass1>();
                    Assert.AreSame(r1, r2);


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
}