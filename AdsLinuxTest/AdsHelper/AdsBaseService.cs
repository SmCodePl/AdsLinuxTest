using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads;

namespace AdsTestLinux.AdsHelper
{
    internal abstract class AdsBaseService : BackgroundService
    {
      protected readonly ILogger logger;
      protected readonly AmsAddress address;
      protected readonly IConfiguration configuration;
      protected AdsClient? _client = null; 

      public AdsBaseService(AmsAddress address, ILogger logger, IConfiguration configuration)
      {
        this.address = address; 
        this.logger = logger;
        this.configuration = configuration;
      }
      ~AdsBaseService()
      {
        Dispose(false);
      }
      bool _disposed = false;
      public override void Dispose()
      {
            if (!_disposed)
                Dispose(true);

            base.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
      }
      protected virtual void Dispose(bool disposing)
      {
            if (disposing)
            {
                if (_client != null)
                    _client.Dispose();
            }
      }
        protected override async Task ExecuteAsync(CancellationToken cancel)
        {
            //Wait for Router to start
            await Task.Delay(TimeSpan.FromSeconds(2),cancel);

            // Establish Connection and read State 
            _client = new AdsClient{ Timeout = 5000 };
            _client.Connect(address);
            
            ResultReadDeviceState result = await _client.ReadStateAsync(CancellationToken.None);
            logger.LogInformation($"Target system '{address}' is in state '{result.State.AdsState}'");

            // Execute the Work handler!
            await OnExecuteAsync(cancel);            
        }
        protected abstract Task OnExecuteAsync(CancellationToken cancellationToken);
    }
}