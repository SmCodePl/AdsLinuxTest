
using System.Configuration;
using AdsTestLinux.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using PlcDataModel.Interfaces;
using TwinCAT.Ads;

namespace AdsTestLinux;
public static class ServiceRegistration
{
    public static IServiceCollection AddSerivces(this IServiceCollection services,IConfiguration configuration)
    {
        string _netId = configuration.GetSection("AmsRouter:RemoteConnections:0:NetId").Value ?? string.Empty;
        //int port; 
        
        AmsNetId netId = new AmsNetId(_netId);
        int.TryParse(configuration.GetSection("AmsRouter:AdsPrort").Value, out int port);
        services.AddHostedService<RouterService>();
        services.AddHostedService<PlcConnectionService>(
            ctx => new PlcConnectionService(new AmsAddress(netId,port),
            ctx.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(PlcConnectionService)),
            configuration
            )
        );
        services.AddHostedService( 
            ctx => new CheckNetworkService(new AmsAddress(netId,port),
            ctx.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(CheckNetworkService)),
            configuration
            ));
        return services;                                                                                                                                                                                                                                                        
    } 
}