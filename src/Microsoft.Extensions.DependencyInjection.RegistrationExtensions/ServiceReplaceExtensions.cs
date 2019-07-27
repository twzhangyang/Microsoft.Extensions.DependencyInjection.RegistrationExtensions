using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection.RegistrationExtensions
{
    public static class ServiceReplaceExtensions
    {
        public static void Replace<TService, TImplement>(this IServiceCollection services, ServiceLifetime lifetime)
        {
            services.Replace(new ServiceDescriptor(typeof(TService), typeof(TImplement), lifetime));
        }

        public static void Replace<TService, TImplement>(this IServiceCollection services,
            Func<IServiceProvider, TImplement> creator, ServiceLifetime lifetime)
        {
            services.Replace(
                new ServiceDescriptor(typeof(TService), container => (object)creator(container), lifetime));
        }

        public static void RemoveByGenericDefinition(this IServiceCollection services, Type type)
        {
            for (int index = services.Count - 1; index >= 0; --index)
            {
                if (services[index].ServiceType.IsGenericType)
                {
                    var genericType = services[index].ServiceType.GetGenericTypeDefinition();
                    if (genericType == type)
                    {
                        services.RemoveAt(index);
                    }
                }
            }
        }
    }
}