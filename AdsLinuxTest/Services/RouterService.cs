using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads.TcpRouter;

namespace AdsTestLinux;

internal class RouterService : BackgroundService
{
    private readonly ILogger<RouterService> _logger;
    private readonly IConfiguration _configuration;

    public RouterService(ILogger<RouterService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Tcp Router");
        
        var router = new AmsTcpIpRouter(_logger,_configuration);
        router.StartAsync(stoppingToken);

        await Task.Delay(1000,stoppingToken);
    }
}