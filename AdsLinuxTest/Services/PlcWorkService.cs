
using System.Diagnostics;
using AdsTestLinux.AdsHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlcDataModel;
using PlcDataModel.Interfaces;
using PlcDataModel.PlcStructure;
using TwinCAT.Ads;

namespace AdsTestLinux.Services;
internal class PlcConnectionService : AdsBaseService, IPlcConnectionService
{
    private readonly IConfiguration _configuration;
    private string headerRead = string.Empty;
    private string headerWrite = string.Empty;
    private string dataRead = string.Empty;
    private string dataWrite = string.Empty;
    private string watchDogNotify = string.Empty;
    public PlcConnectionService(AmsAddress address, ILogger logger, IConfiguration configuration) 
        : base(address, logger,configuration)
    {
        _configuration = configuration;
    }

    public bool GetPlcDataName()
    {
        bool isSucessful = false;
        try
        {
            headerRead = _configuration.GetSection("PlcNode:HeaderRead").Value ?? string.Empty;
            headerWrite = _configuration.GetSection("PlcNode:HeaderWrite").Value ?? string.Empty;
            dataRead = _configuration.GetSection("PlcNode:DataRead").Value ?? string.Empty;
            dataWrite = _configuration.GetSection("PlcNode:DataWrite").Value ?? string.Empty;
            watchDogNotify =_configuration.GetSection("PlcNode:NotificationWatchDog").Value ?? string.Empty;

            isSucessful = true;

        }catch(Exception ex)
        {
            isSucessful = false;
            logger.LogError($"Get Plc Data Structure name error: '{ex}'");
        }

        return isSucessful;
    }
    public void HandleNotificationRequest()
    {
          if(_client!.IsConnected)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            short result = DoMeasurment(headerRead, dataRead, headerWrite, dataWrite, out string readTime, out string writeTime);
            
            stopwatch.Stop();
            Console.WriteLine("Result: " + result);
            if (result != 0)
            {
                TimeSpan timeTaken = stopwatch.Elapsed;
                Console.WriteLine($"Time Taken: {timeTaken}");
                
                logger.LogInformation("{ReadTime}, {WriteTime}, {OperationTimeTaken}", readTime,writeTime,timeTaken);
            }
        }
        else
        {
           logger.LogInformation("PLC is not connected");
        }
    }

    public void RegistrerNofitications()
    {
        try
        {
            if(_client!.IsConnected)
            {
                _client.AdsNotificationEx += OnNotificationReceivedEx;

                var notificationSettings = new NotificationSettings(AdsTransMode.OnChange, 20, 0);
                var variableHandle1 = _client.AddDeviceNotificationEx(watchDogNotify, notificationSettings, null, typeof(short));


                _client.AdsNotificationError += (sender, e) =>
                {
                    Console.WriteLine($"Notification error: {e.Exception}");
                };
            }
        }catch(Exception ex)
        {
            logger.LogError($"PlcConnectionService error: '{ex}'");
        }
    }
    private void OnNotificationReceivedEx(object sender, AdsNotificationExEventArgs e)
    {
        if(e != null)
            HandleNotificationRequest();
    }
    protected short DoMeasurment(string readHeader, string readBody, string writeHeader, string writeBody, out string strReadExedution, out string strWriteExecution)
    {
        return ReadPlcData(readHeader, readBody, writeHeader, writeBody, out strReadExedution, out strWriteExecution);
    }
    private short ReadPlcData(string structureHeader, string structBody, string writeHeader,string writeBody ,out string readTieme, out string writeTime)
    {
        uint handleHeader = _client!.CreateVariableHandle(structureHeader);
        uint handleBody = _client!.CreateVariableHandle(structBody);
        short ret = 0; 
        readTieme = ""; writeTime = "";

        try
        {
            var headerData = (HeaderStruct)_client.ReadAny(handleHeader, typeof(HeaderStruct));
            
            readTieme = headerData.TimeStampLog[0].WriteTiem = TestDataHelper.GetTimeStamp();
            
            var data = (AdsDataType)_client.ReadAny(handleBody, typeof(AdsDataType));
            if (WritePlcData(writeHeader, writeBody,ref data, ref headerData, out writeTime))
            {

            }
            ret = headerData.ItemId;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (handleBody != 0) _client.DeleteVariableHandle(handleBody);
            if (handleHeader != 0) _client.DeleteVariableHandle(handleHeader);

        }
        return ret;
    }

    private bool WritePlcData(string writeHeader,string writeBody, ref AdsDataType body, ref HeaderStruct header, out string writeTime)
    {
        uint writeHeaderHandle = _client!.CreateVariableHandle(writeHeader);
        uint writeBodyHandle = _client.CreateVariableHandle(writeBody);
        writeTime = "";
        try
        {
            if(writeHeaderHandle == 0 || writeBodyHandle == 0) return false;

            TestDataHelper.SetBitInShort(ref header.ItemStatus, 1, true);// = 3;
            _client.WriteAny(writeBodyHandle, body);
           
            writeTime = header.TimeStampLog[0].ReadTime = TestDataHelper.GetTimeStamp(); 
            _client.WriteAny(writeHeaderHandle, header);
            
            return true;

        }catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        finally
        {
            if(writeHeaderHandle != 0) _client.DeleteVariableHandle(writeHeaderHandle);
            if(writeBodyHandle != 0) _client.DeleteVariableHandle(writeBodyHandle);
        } 
    }
    protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        if(GetPlcDataName())
        {
            RegistrerNofitications();
        }
        await Task.Delay(100);
    }
}