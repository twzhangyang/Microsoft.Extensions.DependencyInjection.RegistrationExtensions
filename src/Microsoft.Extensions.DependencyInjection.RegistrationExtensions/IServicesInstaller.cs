namespace Microsoft.Extensions.DependencyInjection.RegistrationExtensions
{
    public interface IServicesInstaller
    {
        void Install(IServiceCollection services);
    }
}