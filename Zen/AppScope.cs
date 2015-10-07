using Autofac;
using System;

namespace Zen
{
    /// <summary>
    ///     Область жизни созданных зависимостей
    ///     При разрушении области жизни разрушаются все зависимости созданные для нее
    /// </summary>
    public class AppScope : IAppScope
    {
        # region Fields

        private ILifetimeScope _scope;

        # endregion

        /// <summary>
        /// Создать область видимости по области видимости контейнера
        /// </summary>
        /// <param name="scope">Ссылка на контейнер Autofac</param>
        public AppScope(ILifetimeScope scope)
        {
            if (scope == null) 
                throw new ArgumentNullException("scope");
            
            Scope = scope;
        }

        protected AppScope()
        {
        }

        /// <summary>
        ///     Ссылка на контейнер Autofac
        /// </summary>
        public ILifetimeScope Scope
        {
            get { return _scope; }
            protected set 
            {
                _scope = value;

                if (_scope != null && !_scope.IsRegistered<IAppScope>())
                {
                    var containerBuilder = new ContainerBuilder();
                    containerBuilder.RegisterInstance(this)
                        .As<IAppScope>()
                        .AsSelf();

                    containerBuilder.Update(_scope.ComponentRegistry);
                }
            }
        }

        /// <summary>
        ///     Разрешить зависимость
        /// </summary>
        /// <typeparam name="TType">Тип зависимости</typeparam>
        /// <returns>Созданный объект</returns>
        public TType Resolve<TType>()
        {
            return Scope.IsRegistered<TType>() ? Scope.Resolve<TType>() : default(TType);
        }

        /// <summary>
        ///     Разрешить зависимость
        /// </summary>
        /// <param name="type">Тип зависимости</param>
        /// <returns>Созданный объект</returns>
        public object Resolve(Type type)
        {
            return Scope.IsRegistered(type) ? Scope.Resolve(type) : null;
        }

        /// <summary>
        ///     Сощдать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
        public AppScope BeginScope()
        {
            var scope = new AppScope();
            scope.Scope = Scope.BeginLifetimeScope(scope, InnerScopeRegistratorBuilder(scope));
            
            return scope;
        }

        /// <summary>
        ///     Сощдать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
        public AppScope BeginScope(Action<ContainerBuilder> confAction)
        {
            var scope = new AppScope();
            scope.Scope = Scope.BeginLifetimeScope(scope, InnerScopeRegistratorBuilder(scope, confAction));
            
            return scope;
        }

        public AppScope BeginScope(object tag)
        {
            var scope = new AppScope();
            scope.Scope = Scope.BeginLifetimeScope(tag, InnerScopeRegistratorBuilder(scope));

            return scope;
        }

        public AppScope BeginScope(object tag, Action<ContainerBuilder> confAction)
        {
            var scope = new AppScope();
            scope.Scope = Scope.BeginLifetimeScope(tag, InnerScopeRegistratorBuilder(scope, confAction));

            return scope;
        }

        public virtual void Dispose()
        {
            if (Scope != null)
            {
                Scope.Dispose();
                Scope = null;
            }
        }

        protected Action<ContainerBuilder> InnerScopeRegistratorBuilder(AppScope scope, Action<ContainerBuilder> configurationAction = null)
        {
            return b =>
            {
                b.RegisterInstance(scope)
                    .As<IAppScope>()
                    .AsSelf();

                if (configurationAction != null)
                    configurationAction(b);
            };
        }
    }
}