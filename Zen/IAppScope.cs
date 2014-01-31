using System;
using Autofac;

namespace Zen
{
    /// <summary>
    ///     Область жизни созданных зависимостей
    ///     При разрушении области жизни разрушаются все зависимости созданные для нее
    /// </summary>
    public interface IAppScope : IDisposable
    {
        /// <summary>
        ///     Разрешить зависимость
        /// </summary>
        /// <typeparam name="TType">Тип зависимости</typeparam>
        /// <returns>Созданный объект</returns>
        TType Resolve<TType>();

        /// <summary>
        ///     Разрешить зависимость
        /// </summary>
        /// <param name="type">Тип зависимости</param>
        /// <returns>Созданный объект</returns>
        object Resolve(Type type);

        /// <summary>
        ///     Создать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
		IAppScope BeginScope();

        /// <summary>
        ///     Создать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
		IAppScope BeginScope(Action<ContainerBuilder> confAction);

        /// <summary>
        ///     Создать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
		IAppScope BeginScope(object tag);

        /// <summary>
        ///     Создать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
		IAppScope BeginScope(object tag, Action<ContainerBuilder> confAction);

		/// <summary>
		///     Ссылка на контейнер Autofac.
		/// TODO: Убрать зависимость интерфейса ILifetimeScope от Autofac. Возможно нужно сделать IAppScope базовым, добавить IAppScope&lt;T&gt; и реализацию IAppScope&lt;ILifetimeScope&gt;
		/// </summary>
		ILifetimeScope Scope { get; }
    }
}