using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Autofac;

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
            {
                function = Transformers[t];
            }
            builder
                .Register(c => function(c.Resolve(factoryType)))
                .As(t);
        }

        /// <summary>
        /// Зарегистрировать интерфейс для фабрики интерфейсов
        /// </summary>
        /// <typeparam name="T">Интерфейс</typeparam>
        /// <param name="builder">Построитель контейнера</param>
        public static void RegisterInterfaceForEmit<T>(this ContainerBuilder builder)
        {
            builder
                .Register(c => c.Resolve<EmitInterfaceImplementor<T>>().ImplementInterface())
                .As<T>();
        }        
    }
}