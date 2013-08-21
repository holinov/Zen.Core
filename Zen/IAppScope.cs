using System;

namespace Zen
{
    public interface IAppScope : IDisposable
    {
        TType Resolve<TType>();
        object Resolve(Type type);
        AppScope BeginScope();
    }
}