using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace WebAppExam.InventoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            cfg.RegisterServicesFromAssemblies(assemblies);
        });

        return services;
    }
}
