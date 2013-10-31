using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using System.Web.Mvc;

namespace Zen.Core.MVC4
{
    public class ZenMvcDependencyScopeResolver : IDependencyScope
    {
        private static AppScope _appScope;
        private readonly IAppScope _scope;


        public ZenMvcDependencyScopeResolver(IAppScope scope)
        {
            //Всегда создаем дочернюю область видимости
            _scope = scope.BeginScope();
        }

        public static AppScope AppScope
        {
            get { return _appScope??DependencyResolver.Current.GetService<AppCore>(); }
            set { _appScope = value; }
        }


        /// <summary>
        /// Resolves singly registered services that support arbitrary object creation.
        /// </summary>
        /// <returns>
        /// The requested service or object.
        /// </returns>
        /// <param name="serviceType">The type of the requested service or object.</param>
        public object GetService(Type serviceType)
        {
            try
            {
                return AppScope.Resolve(serviceType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves a collection of services from the scope.
        /// </summary>
        /// <returns>
        /// The retrieved collection of services.
        /// </returns>
        /// <param name="serviceType">The collection of services to be retrieved.</param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                var servicesType = typeof(IEnumerable<>);
                var realServiceTypes = servicesType.MakeGenericType(serviceType);
                return (IEnumerable<object>)_scope.Resolve(realServiceTypes);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual void Dispose()
        {
            _scope.Dispose();
        }
    }
}