using System.Globalization;
using AdsTestLinux.AdsHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads;

namespace AdsTestLinux
{
    internal class CheckNetworkService : AdsBaseService
    {
        private readonly string _nCheckNetwork = "";
        private readonly string _echoCheckNetwork = "";
         private uint readHandle = 0;
        private uint writeHandle = 0;
        private short pcCheckNetwork = 0;
        //"Global.stAdsCheckNetwork.nCheckNetwork";//"Global.stAdsCheckNetwork.nEchoCheckNetwork";//
        public CheckNetworkService(AmsAddress address, ILogger logger, IConfiguration configuration) 
        : base(address, logger,configuration)
        {
            _nCheckNetwork    =  configuration.GetSection("PlcNode:CheckNetwork").Value ?? string.Empty;
            _echoCheckNetwork = configuration.GetSection("PlcNode:EchoCheckNetwork").Value ?? string.Empty;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancel)
        {
            while(!cancel.IsCancellationRequested)
            {
                if(_client != null && _client.IsConnected)
                {
                    await CheckNetwork();
                }
                else 
                    logger.LogError($"Cannot get ProjectName from target '{address}'");
            }
        }
         private async Task CheckNetwork()
        {
            try
            {
                if (_client!.IsConnected)
                {
                    if (readHandle != 0 && writeHandle != 0)
                    {
                        pcCheckNetwork = (short)_client.ReadAny(readHandle, typeof(short));
                        await _client.WriteAnyAsync(writeHandle, pcCheckNetwork,CancellationToken.None);
                    }
                    else
                    {
                        SetHandle();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error reading from PLC '{address}' , exception: '{ex}'");

            }
        }
        private void SetHandle()
        {
            try
            {
                if(readHandle == 0 )    readHandle  = _client!.CreateVariableHandle(_nCheckNetwork);
                if(writeHandle == 0 )   writeHandle = _client!.CreateVariableHandle(_echoCheckNetwork);

            }catch(Exception)
            {
                logger.LogCritical($"Cannot register PLC variables checkNetwork: '{_nCheckNetwork}' and echocheckNetwork: '{_echoCheckNetwork}'");
            }

        }
    }
}