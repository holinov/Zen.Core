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
        AppScope BeginScope();

        /// <summary>
        ///     Создать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
        AppScope BeginScope(Action<ContainerBuilder> confAction);

        /// <summary>
        ///     Создать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
        AppScope BeginScope(object tag);

        /// <summary>
        ///     Создать вложенную область видимости
        /// </summary>
        /// <returns>Новая область видимости</returns>
        AppScope BeginScope(object tag, Action<ContainerBuilder> confAction);

        /// <summary>
        /// Обновить регистрации в текущем контексте
        /// </summary>
        /// <param name="cb">Билдер содержащий нужные обновления</param>
        void Update(ContainerBuilder cb);
    }
}