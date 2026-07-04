using System;
using System.Collections.Generic;

namespace Systems
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IDisposable> Services = new();
        public static void Reset()
        {
            foreach (var service in Services)
            {
                service.Value.Dispose();
            }
            Services.Clear();
        }

        #region Service Management

        public static void Register<TService>(TService service) where TService : class, IDisposable
        {
            Services.Add(typeof(TService), service);
        }
        
        public static void Unregister<TService>() where TService : class, IDisposable
        {
            Services.Remove(typeof(TService));
        }

        
        
        #endregion

        #region Service Providers

        public static T GetService<T>() where T : class, IDisposable
        {
            if(Services.TryGetValue(typeof(T), out var service) && service is T value)return value;
            return null;
        }

        #endregion
    }
}
