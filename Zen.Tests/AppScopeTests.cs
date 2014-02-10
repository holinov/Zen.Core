﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Autofac;
using Autofac.Core;

using NHibernate.Criterion;

using NUnit.Framework;

namespace Zen.Tests
{
    [TestFixture]
    public class AppScopeTests
    {
        public class TestClass1
        {
            public int Val { get; set; }

	        public string SomeMethod(int a, int b)
	        {
		        return "test ok "+(a+b);
	        }
        }
		public class TestClass1b : IInterfaceWithMethod
		{
			private IAppScope _scope;

			public string SomeMethod(int a, int b)
			{
				return _scope.Resolve<TestClass1>().SomeMethod(a, b);
			}
			
		}
        public class TestClass2:IDisposable
        {
            public int Val { get; set; }
            public void Dispose()
            {
               Trace.WriteLine("Dispose");
            }
			public string SomeMethod(int a, string b)
			{
				return "test ok";
			}
        }


        public interface IInterfaceConfig
        {
            Config Config { get; }
        }
		public interface IInterfaceWithMethod 
		{
			[MethodProxy(typeof(TestClass1))]
			string SomeMethod(int a, int b);
		}
		public interface IInterfaceWithMethodF
		{
			[MethodProxy(typeof(TestClass2))]
			string SomeMethod(int a, int b);
		}
        public interface IInterface : IInterfaceConfig
        {
            TestClass1 Test1 { get; }
            TestClass2 Test2 { get; }
        }
        public interface IInterface1 : IInterface,IDisposable
        {
            
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
                .Configure(b => b.RegisterInterfaceForEmit<IInterface1>())
                .Configure(b => b.RegisterType<Config>().AsSelf().SingleInstance())
                .Build())
            {

                using (var lscope = core.Scope.BeginLifetimeScope())
                {
                    var t2 = lscope.Resolve<TestClass2>();
                    t2.Val = 1;
                }

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

                    using (var vaf2 = scope.Resolve<IInterface1>())
                    {
                        Assert.AreNotSame(vaf2.Test2, vaf1.Test2);
                    }
                }
            }
        }
		[Test]
		public void IInterfaceWithMethodTest()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<TestClass1>().AsSelf().SingleInstance();
			builder.RegisterModule<EmitImplementerModule>();
			builder.RegisterType<TestClass2>().AsSelf().InstancePerLifetimeScope();
		
			using (
				var core =
					AppCoreBuilder.Create(builder)
						.AddModule<EmitImplementerModule>()
						.Configure(b => b.RegisterInterfaceForEmit<IInterfaceWithMethod>())
						.Configure(b => b.RegisterType<Config>().AsSelf().SingleInstance())
						.Build())
			{
				using (var scope = core.BeginScope())
				{
					var resolvedScope = scope.Resolve<IInterfaceWithMethod>();
					Assert.AreEqual("test ok 3", resolvedScope.SomeMethod(1,2));
				}
			}
		}
		[Test]
		public void IInterfaceWithMethodTestFail()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<TestClass1>().AsSelf().SingleInstance();
			builder.RegisterModule<EmitImplementerModule>();
			builder.RegisterType<TestClass2>().AsSelf().InstancePerLifetimeScope();

			using (
				var core =
					AppCoreBuilder.Create(builder)
						.AddModule<EmitImplementerModule>()
						.Configure(b => b.RegisterInterfaceForEmit<IInterfaceWithMethodF>())
						.Configure(b => b.RegisterType<Config>().AsSelf().SingleInstance())
						.Build())
			{
				using (var scope = core.BeginScope())
				{
					Assert.Throws<DependencyResolutionException>(() => scope.Resolve<IInterfaceWithMethodF>());
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