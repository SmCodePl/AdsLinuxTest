using Microsoft.Extensions.DependencyInjection;
using OpcLinuxTest.Services;

namespace OpcLinuxTest;
public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection service)
    {
        service.AddSingleton<OpcUaTestService>();
        service.AddHostedService<OpcWorker>();
        return service;
    }
}