using System;
using Autofac;

namespace Zen
{
    /// <summary>
    ///     Область жизни созданных зависимостей
    ///     При разрушении области жизни разрушаются все зависимости созданные для нее
    /// TODO: Реализация AppScopre зависит от Autofac. Переименовать её в AutofacAppScope, отрефакторить код, позволив существовать другим реализациям.
    /// </summary>
    public class AppScope : IAppScope
    {
		/// <summary>
		/// Признак того, что AppScope корректно закрыт.
		/// </summary>
	    private bool isDisposed;

        /// <summary>
        /// Создать область видимости по области видимости контейнера
        /// </summary>
        /// <param name="scope">Ссылка на контейнер Autofac</param>
        public AppScope(ILifetimeScope scope)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            Scope = scope;
        }

        protected AppScope()
        {
        }

		/// <summary>
		/// Финализатор AppScope. Проверяет корректность закрытия скопа.
		/// </summary>
		~AppScope()
		{
			if (!isDisposed)
				Console.Error.WriteLine("AppScope wasn't disposed. Please use using keyword!");
		}

        /// <summary>
        ///     Ссылка на контейнер Autofac
        /// </summary>
        public ILifetimeScope Scope { get; protected set; }

        /// <summary>
        ///     Разрешить зависимость
        /// </summary>
        /// <typeparam name="TType">Тип зависимости</typeparam>
        /// <returns>Созданный объект</returns>
        public TType Resolve<TType>()
        {
            return Scope.Resolve<TType>();
        }

        /// <summary>
        ///     Разрешить зависимость
        /// </summary>
        /// <param name="type">Тип зависимости</param>
        /// <returns>Созданный объект</returns>
        public object Resolve(Type type)
        {
            if (!Scope.IsRegistered(type)) return null;
            return Scope.Resolve(type);
        }

        /// <summary>
        ///     Сощдать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
		public IAppScope BeginScope()
        {
            var scope = new AppScope();
            var lscope = Scope.BeginLifetimeScope(b => b.Register(c => scope)
                .As<IAppScope>()
                .AsSelf()
                .InstancePerLifetimeScope());
            scope.Scope = lscope;
            return scope;
        }

        /// <summary>
        ///     Сощдать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
		public IAppScope BeginScope(Action<ContainerBuilder> confAction)
        {
            var scope = new AppScope();
            var lscope = Scope.BeginLifetimeScope(scope,b =>
                {
                    b.Register(c => scope)
                     .As<IAppScope>()
                     .AsSelf()
                     .InstancePerLifetimeScope();
                    confAction(b);
                });
            scope.Scope = lscope;
            return scope;
        }

		public IAppScope BeginScope(object tag)
        {
            var scope = new AppScope();
            var lscope = Scope.BeginLifetimeScope(tag,b => b.Register(c => scope)
                                                            .As<IAppScope>()
                                                            .AsSelf()
                                                            .InstancePerLifetimeScope());
            scope.Scope = lscope;
            return scope;
        }

		public IAppScope BeginScope(object tag, Action<ContainerBuilder> confAction)
        {
            var scope = new AppScope();
            var lscope = Scope.BeginLifetimeScope(tag,b =>
            {
                b.Register(c => scope)
                 .As<IAppScope>()
                 .AsSelf()
                 .InstancePerLifetimeScope();
                confAction(b);
            });
            scope.Scope = lscope;
            return scope;
        }

        public virtual void Dispose()
        {
	        isDisposed = true;
            if (Scope != null)
            {
                //Scope.Dispose();
                Scope = null;
            }
        }
    }
}