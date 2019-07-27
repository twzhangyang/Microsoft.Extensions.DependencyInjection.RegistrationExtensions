using System;

namespace Microsoft.Extensions.DependencyInjection.RegistrationExtensions
{
    public static class ServiceProviderExtensions
    {
        public static T ResolveService<T>(this IServiceProvider provider)
            where T : class
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return (T)provider.GetService(typeof(T));
        }
    }
}