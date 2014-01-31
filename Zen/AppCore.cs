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
        private IContainer _container;
        public static AppCore Instance { get; private set; }
        /// <summary>
        /// Создать область видимости приложения
        /// </summary>
        /// <param name="container">Контейнер области видимости приложения</param>
        public AppCore(ILifetimeScope container)
            : base()
        {
            _rootScope = container;
            _container = container as IContainer;
            Scope =
                _rootScope.BeginLifetimeScope(
                    b =>
                        {
                            b.Register(c => this).SingleInstance().AsSelf();
                            b.RegisterType<Config>().SingleInstance().AsSelf();
                            b.Register(c => this.BeginScope()).InstancePerDependency().As<IAppScope>().AsSelf();
                            b.RegisterModule<EmitImplementerModule>();
                        });
            Instance = this;
        }



        public override void Dispose()
        {
            base.Dispose();
            _rootScope.Dispose();
        }
    }
}