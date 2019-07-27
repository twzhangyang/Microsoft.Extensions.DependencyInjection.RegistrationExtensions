using System;

namespace Microsoft.Extensions.DependencyInjection.RegistrationExtensions
{
    public class ServiceReference
    {
        public Type Service { get; set; }

        public Type Implementation { get; set; }
    }
}