
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Ua.Client;
using OpcLinuxTest.Services;

namespace OpcLinuxTest
{
    internal class OpcWorker : BackgroundService
    {
        private readonly ILogger<OpcWorker> _logger;
        private readonly OpcUaTestService _testService;
        private Session? _session;
        public OpcWorker(ILogger<OpcWorker> logger, OpcUaTestService testService)
        {
            _logger = logger;
            _testService = testService;
        }

         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
         {
            while (!stoppingToken.IsCancellationRequested)
            {
                if(_session != null && _session.Connected)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    await _testService.NetworkCheck().ConfigureAwait(false);
                    _testService.DoEvent();
                    stopwatch.Stop();

                    TimeSpan timeTaken = stopwatch.Elapsed;
                    Console.WriteLine($"all Operation: {timeTaken}");
                }
                else 
                {
                    await ConnectToOpcServerAsync();
                }
              
            }
         }
        private async Task ConnectToOpcServerAsync()
        {
            try
            {
                
                 _session = await _testService.Connect();
                 _ = Task.Run(async () => await _testService.NetworkCheck());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while connecting to OPC server, error: '{ex}'");
            }

        }
    }
}