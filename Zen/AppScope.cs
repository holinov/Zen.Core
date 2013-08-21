using System;
using Autofac;

namespace Zen
{
    public class AppScope : IAppScope
    {
        private readonly ILifetimeScope _scope;

        public AppScope(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public TType Resolve<TType>()
        {
            return _scope.Resolve<TType>();
        }

        public object Resolve(Type type)
        {
            return _scope.Resolve(type);
        }

        public AppScope BeginScope()
        {
            return new AppScope(_scope.BeginLifetimeScope());
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public ILifetimeScope Scope { get { return _scope; } }
    }
}