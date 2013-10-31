using Autofac;

namespace Zen
{
    /// <summary>
    ///     Класс ядра приложения
    ///     Представляет собой глобальную область видимости
    /// </summary>
    public class AppCore : AppScope
    {
        private readonly ILifetimeScope _rootScope;

        /// <summary>
        /// Создать область видимости приложения
        /// </summary>
        /// <param name="container">Контейнер области видимости приложения</param>
        public AppCore(ILifetimeScope container)
            : base()
        {
            _rootScope = container;
            Scope =
                _rootScope.BeginLifetimeScope(
                    b => b.Register(c => this).SingleInstance().AsSelf());
        }

        public override void Dispose()
        {
            base.Dispose();
            _rootScope.Dispose();
        }
    }
}