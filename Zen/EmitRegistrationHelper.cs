using Autofac;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zen
{
    /// <summary>
    /// Класс помошник регистрации интерфесов для EmmitInterfaceImplementer
    /// </summary>
    public static class EmitRegistrationHelper
    {
        private static readonly Dictionary<Type, Func<object, object>> Transformers = new Dictionary<Type, Func<object, object>>();
        
        /// <summary>
        /// Зарегистрировать несколько интерфейсов для фабрики интерфейсов
        /// </summary>
        /// <param name="builder">Построитьель контейнера</param>
        /// <param name="types">Интерфейсы для регистрации</param>
        public static void RegisterInterfacesForEmit(this ContainerBuilder builder,params Type[] types)
        {
            foreach (var type in types)
            {
                builder.RegisterInterfaceForEmit(type);
            }
        }        

        /// <summary>
        /// Зарегистрировать интерфейс для фабрики интерфейсов
        /// </summary>
        /// <param name="builder">Построитель контейнера</param>
        /// <param name="t">Интерфейс</param>
        public static void RegisterInterfaceForEmit(this ContainerBuilder builder,Type t)
        {
            Func<object,object> function;
            var factoryType = typeof(EmitInterfaceImplementor<>).MakeGenericType(t);

            if (!Transformers.ContainsKey(t))
            {
                var expression2 = Expression.Parameter(typeof (object), "emi");
                var methodInfo = factoryType.GetMethod("ImplementInterface");
                Expression<Func<object,object>> expression =
                    Expression.Lambda<Func<object, object>>(
                        Expression.Call(Expression.Convert(expression2, factoryType), methodInfo, new Expression[0]),
                        new[] {expression2});
                function = expression.Compile();
                Transformers[t] = function;

            }
            else
                function = Transformers[t];
            
            builder.Register(c => function(Activator.CreateInstance(factoryType, AppScopeResolver(c)))).As(t);
        }

        /// <summary>
        /// Зарегистрировать интерфейс для фабрики интерфейсов
        /// </summary>
        /// <typeparam name="T">Интерфейс</typeparam>
        /// <param name="builder">Построитель контейнера</param>
        public static void RegisterInterfaceForEmit<T>(this ContainerBuilder builder)
        {
            builder.Register(c => new EmitInterfaceImplementor<T>(AppScopeResolver(c)).ImplementInterface()).As<T>();
        }

        private static IAppScope AppScopeResolver(IComponentContext ctx)
        {
            if (ctx.IsRegistered<IAppScope>())
                return ctx.Resolve<IAppScope>();

            if (ctx.IsRegistered<ILifetimeScope>())
                return new AppScope(ctx.Resolve<ILifetimeScope>());

            if (ctx.IsRegistered<AppCore>())
                return ctx.Resolve<AppCore>();

            return null;
        }
    }
}