using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.RegistrationExtensions
{
    public static class FromAssembly
    {
        private static readonly ConcurrentDictionary<Assembly, List<IServicesInstaller>> _concurrentDictionary =
            new ConcurrentDictionary<Assembly, List<IServicesInstaller>>();

        public static List<IServicesInstaller> Contains<T>()
        {
            var assembly = typeof(T).GetTypeInfo().Assembly;

            return _concurrentDictionary.GetOrAdd(assembly, ScanInstallers);
        }

        public static List<IServicesInstaller> This()
        {
            var assembly = Assembly.GetEntryAssembly();

            return ScanInstallers(assembly);
        }

        private static List<IServicesInstaller> ScanInstallers(Assembly assembly)
        {
            var installers = assembly.ExportedTypes
                .Where(t => typeof(IServicesInstaller).IsAssignableFrom(t))
                .Where(t => t.IsClass)
                .Select(Activator.CreateInstance)
                .Select(i => i as IServicesInstaller)
                .ToList();

            return installers;
        }
    }
}