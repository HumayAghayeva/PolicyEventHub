using DataBridge.API.Services;

namespace DataBridge.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
     
           services.Scan(scan => scan
    .FromAssemblies(typeof(CompulsoryCanceledPolicyService).Assembly)
    .AddClasses(c => c.InNamespaces("DataBridge.API"))
    .AsMatchingInterface()
    .WithScopedLifetime());

            return services;
        }
    }
}

