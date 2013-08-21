using System;
using Autofac;
using Autofac.Core;

namespace Zen
{
    public class AppCoreBuilder
    {
        private readonly ContainerBuilder _builder;

        public AppCoreBuilder(ContainerBuilder builder)
        {
            _builder = builder;
        }

        public AppCoreBuilder AddModule(Module module)
        {
            _builder.RegisterModule(module);
            return this;
        }

        public AppCoreBuilder AddModule<TModule>() where TModule : IModule, new()
        {
            _builder.RegisterModule<TModule>();
            return this;
        }

        public AppCoreBuilder Configure(Action<ContainerBuilder> configAction)
        {
            configAction(_builder);
            return this;
        }

        public AppCore Build()
        {
            var container = _builder.Build();
            return new AppCore(container);
        }

        public static AppCoreBuilder Create(ContainerBuilder builder = null)
        {
            return new AppCoreBuilder(builder??new ContainerBuilder());
        }
    }
}