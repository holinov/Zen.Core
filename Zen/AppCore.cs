using Autofac;

namespace Zen
{
    /// <summary>
    ///     Класс ядра приложения
    ///     Представляет собой глобальную область видимости
    /// </summary>
    public class AppCore : AppScope
    {
        public static AppCore Instance { get; private set; }

        private readonly ILifetimeScope _rootScope;
        
        /// <summary>
        /// Создать область видимости приложения
        /// </summary>
        /// <param name="container">Контейнер области видимости приложения</param>
        public AppCore(ILifetimeScope container) : base()
        {
            _rootScope = container;
            
            Scope = _rootScope.BeginLifetimeScope(b =>
                {
                    b.RegisterType<Config>()
                        .AsSelf()
                        .SingleInstance();

                    b.RegisterInstance(this)
                        .AsSelf()
                        .SingleInstance();

                    b.Register(c => this.BeginScope())
                        .As<IAppScope>()
                        .AsSelf();

                    b.RegisterModule<EmitImplementerModule>();
                });
            
            Instance = this;
        }

        public void Update(ContainerBuilder cb)
        {
            if (_rootScope == null)
                throw new ZenCoreException("Ядро Zen.Core не получило контейнер при построении. Функционал не доступен.");
            
            cb.Update(_rootScope.ComponentRegistry);
        }

        public override void Dispose()
        {
            base.Dispose();
            _rootScope.Dispose();
        }
    }
}