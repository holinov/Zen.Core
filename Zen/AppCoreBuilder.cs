using System;
using Autofac;
using Autofac.Core;

namespace Zen
{
    /// <summary>
    ///     Построитель ядра приложения
    /// </summary>
    public class AppCoreBuilder
    {
        private readonly ContainerBuilder _builder;

        /// <summary>
        /// Создать построитель ядра
        /// </summary>
        /// <param name="builder">Построитель контейнера Autofac</param>
        public AppCoreBuilder(ContainerBuilder builder)
        {
            _builder = builder;
        }

        /// <summary>
        ///     Добавить модуль
        /// </summary>
        /// <param name="module">Экземпляр модуля</param>
        /// <returns>Построитель ядра</returns>
        public AppCoreBuilder AddModule(Module module)
        {
            _builder.RegisterModule(module);
            return this;
        }

        /// <summary>
        ///     Добавить модуль
        /// </summary>
        /// <typeparam name="TModule">Тип модуля</typeparam>
        /// <returns>Построитель ядра</returns>
        public AppCoreBuilder AddModule<TModule>() where TModule : IModule, new()
        {
            _builder.RegisterModule<TModule>();
            return this;
        }

        /// <summary>
        ///     Произвести произвольную конфигурацию Autofac ContainerBuilder
        /// </summary>
        /// <param name="configAction">Действие конфигурации</param>
        /// <returns>Построитель ядра</returns>
        public AppCoreBuilder Configure(Action<ContainerBuilder> configAction)
        {
            configAction(_builder);
            return this;
        }

        /// <summary>
        ///     Построить ядро приложения
        /// </summary>
        /// <returns>Ядро приложения</returns>
        public AppCore Build()
        {
            IContainer container = _builder.Build();
            return new AppCore(container);
        }

        /// <summary>
        ///     Получить экземпляр построителя ядра
        /// </summary>
        /// <param name="builder">Конфигуратор контейнера</param>
        /// <returns>Построитель ядра</returns>
        public static AppCoreBuilder Create(ContainerBuilder builder = null)
        {
            return new AppCoreBuilder(builder ?? new ContainerBuilder());
        }
    }
}