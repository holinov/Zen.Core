using System;
using Autofac;

namespace Zen
{
    /// <summary>
    ///     Область жизни созданных зависимостей
    ///     При разрушении области жизни разрушаются все зависимости созданные для нее
    /// </summary>
    public class AppScope : IAppScope
    {
        private readonly ILifetimeScope _scope;

        public AppScope(ILifetimeScope scope)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            _scope = scope;
        }

        /// <summary>
        ///     Ссылка на контейнер Autofac
        /// </summary>
        public ILifetimeScope Scope
        {
            get { return _scope; }
        }

        /// <summary>
        ///     Разрешить зависимость
        /// </summary>
        /// <typeparam name="TType">Тип зависимости</typeparam>
        /// <returns>Созданный объект</returns>
        public TType Resolve<TType>()
        {
            return _scope.Resolve<TType>();
        }

        /// <summary>
        ///     Разрешить зависимость
        /// </summary>
        /// <param name="type">Тип зависимости</param>
        /// <returns>Созданный объект</returns>
        public object Resolve(Type type)
        {
            return _scope.Resolve(type);
        }

        /// <summary>
        ///     Сощдать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
        public AppScope BeginScope()
        {
            return new AppScope(_scope.BeginLifetimeScope());
        }

        /// <summary>
        ///     Сощдать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
        public AppScope BeginScope(Action<ContainerBuilder> confAction)
        {
            return new AppScope(_scope.BeginLifetimeScope(confAction));
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}