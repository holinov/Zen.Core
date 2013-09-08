using Autofac;

namespace Zen
{
    /// <summary>
    ///     Класс ядра приложения
    ///     Представляет собой глобальную область видимости
    /// </summary>
    public class AppCore : AppScope
    {
        public AppCore(ILifetimeScope container)
            : base(container)
        {
        }
    }
}