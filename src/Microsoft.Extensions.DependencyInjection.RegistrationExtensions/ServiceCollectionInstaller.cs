using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.RegistrationExtensions
{
    public static class ServiceCollectionInstaller
    {
        public static void Install(this IServiceCollection serviceCollection, List<IServicesInstaller> installers)
        {
            installers.ForEach(i => i.Install(serviceCollection));
        }

        public static void AddScoped(this IServiceCollection services, List<ServiceReference> servicePairs)
        {
            servicePairs.ForEach(s => services.AddScoped(s.Service, s.Implementation));
        }

        public static void AddSingleton(this IServiceCollection services, List<ServiceReference> servicePairs)
        {
            servicePairs.ForEach(s => services.AddSingleton(s.Service, s.Implementation));
        }
    }
}