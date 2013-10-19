using Autofac;

namespace Zen
{
    /// <summary>
    ///     Класс ядра приложения
    ///     Представляет собой глобальную область видимости
    /// </summary>
    public class AppCore : AppScope
    {
        /// <summary>
        /// Создать область видимости приложения
        /// </summary>
        /// <param name="container">Контейнер области видимости приложения</param>
        public AppCore(ILifetimeScope container)
            : base(container)
        {
        }
    }
}