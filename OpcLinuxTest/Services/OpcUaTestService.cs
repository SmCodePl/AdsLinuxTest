using Opc.Ua;
using Opc.Ua.Client;
using System.Diagnostics;
using Opc.Ua.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using OpcLinuxTest.Model;
using OpcLinuxTest.Interfaces;
using PlcDataModel;
using System.Security.Cryptography.X509Certificates;

namespace OpcLinuxTest.Services;
internal class OpcUaTestService: IOpcUaTest
{
        private readonly ILogger<OpcUaTestService> _logger ;
        private readonly IConfiguration _configuration ;

        private Session _session = null!;
        DataValue checkNetworkValue = new DataValue();
        DataValue echoCheckNetworkValue = new DataValue();
        StatusCodeCollection status;
        DiagnosticInfoCollection results;
        private short _itmId = 0;
        private byte ItmStatus = 0;
        private List<OpcTopic> _opcTopics = new List<OpcTopic>()
        {
            new OpcTopic() {Id = 1, Name = "aVariableTypeBool",     TopicRead = "ns=4;s=Global.stOpcTxData.stDataPayload.aVariableTypeBool",    TopicWrite = "ns=4;s=Global.stOpcRxData.stDataPayload.aVariableTypeBool" },
            new OpcTopic() {Id = 2, Name = "aVariableTypeInt",      TopicRead = "ns=4;s=Global.stOpcTxData.stDataPayload.aVariableTypeInt",     TopicWrite = "ns=4;s=Global.stOpcRxData.stDataPayload.aVariableTypeInt" },
            new OpcTopic() {Id = 3, Name = "aVariableTypeReal",     TopicRead = "ns=4;s=Global.stOpcTxData.stDataPayload.aVariableTypeReal",    TopicWrite = "ns=4;s=Global.stOpcRxData.stDataPayload.aVariableTypeReal" },
            new OpcTopic() {Id = 4, Name = "aVariableTypeString",   TopicRead = "ns=4;s=Global.stOpcTxData.stDataPayload.aVariableTypeString",  TopicWrite = "ns=4;s=Global.stOpcRxData.stDataPayload.aVariableTypeString" },
        };

        private List<OpcConfirm> opcConfirms = new List<OpcConfirm>()
        {
            new OpcConfirm() {Id = 1, Name = "sRxTimeStamp",    TopicWrite = "ns=4;s=Global.stOpcRxData.stTimeLog.sRxTimeStamp" },
            new OpcConfirm() {Id = 2, Name = "sTxTimeStamp",    TopicWrite = "ns=4;s=Global.stOpcRxData.stTimeLog.sTxTimeStamp" },
            new OpcConfirm() {Id = 3, Name = "nItemID",         TopicWrite = "ns=4;s=Global.stOpcRxData.nItemID" },
            new OpcConfirm() {Id = 3, Name = "nItemStatus",     TopicWrite = "ns=4;s=Global.stOpcRxData.nItemStatus" }
        };
        public OpcUaTestService(ILogger<OpcUaTestService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public  Task<Session> Connect()
        {
            string userName = _configuration.GetSection("OpcConfig:UserName").Value ?? string.Empty;
            string password = _configuration.GetSection("OpcConfig:Password").Value ?? string.Empty;
            string opcEndPoint = _configuration.GetSection("OpcConfig:OpcEndpoint").Value ?? string.Empty;
            bool useCredentials = _configuration.GetSection("OpcConfig:UseCredentials").Value == "true" ? true : false;

            try
            {
                var config = new ApplicationConfiguration()
                {
                    ApplicationName = "MyClient",
                    ApplicationUri = Utils.Format(@"urn:{0}:MyClient", System.Net.Dns.GetHostName()),
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"Repository\Cert\MachineDefault", SubjectName = "MyClientSubjectName" },
                        TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"Repository\Cert\UA Certificate Authorities" },
                        TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"Repository\Cert\UA Applications" },
                        RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"Repository\Cert\RejectedCertificates" },
                        AutoAcceptUntrustedCertificates = true
                    },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                    TraceConfiguration = new TraceConfiguration()
                };

                config.Validate(ApplicationType.Client).GetAwaiter().GetResult();

                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
                }

                var application = new ApplicationInstance
                {
                    ApplicationName = "MyClient",
                    ApplicationType = ApplicationType.Client,
                    ApplicationConfiguration = config
                };

                EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(opcEndPoint, false); 
                EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(config);
                ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                UserIdentity user = new UserIdentity();
                
                if (useCredentials)
                {
                    user = new UserIdentity(userName, password );
                }

                // Sets whether or not the discovery endpoint is used to update the endpoint description before connecting.
                bool updateBeforeConnect = false;

                // Sets whether or not the domain in the certificate must match the endpoint used
                bool checkDomain = false;

                // The name to assign to the session
                string sessionName = config.ApplicationName;

                // The session's timeout interval
                uint sessionTimeout = 60000;

                // List of preferred locales
                List<string> preferredLocales = null;

               // var appCertificate = new X509Certificate2(@"./Repository/Cert/endpoint.key");
               // application.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate = new(appCertificate);
                try
                {
                    // Create the session
                    _session = Session.Create(
                                 config,
                                 endpoint,
                                 updateBeforeConnect,
                                 checkDomain,
                                 sessionName,
                                 sessionTimeout,
                                 user,
                                 preferredLocales
                             ).Result;

                    return Task.FromResult(_session);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return default!;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return default!;
            }
        }
        public Task Disconnect()
        {
            if (_session != null && _session.Connected)
            {
                Task.FromResult(_session.Close());
            }
            
            return Task.CompletedTask;
        }

        public void DoEvent()
        {
            throw new NotImplementedException();
        }
        protected void HandleRequest()        
        {
            if (_session != null && _session.Connected)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                if (GetPlcRequest(out short itmId, out byte ItmStatus))
                {
                    string readTieme = TestDataHelper.GetTimeStamp();

                    foreach (var item in _opcTopics)
                    {
                        PlcProcessHandle(item.TopicRead, item.TopicWrite);
                    }

                    foreach (var item in opcConfirms)
                    {
                        if (item.Name == "sRxTimeStamp")
                        {
                            PlcWriteOpcHeaderData(item.TopicWrite, readTieme);
                        }
                        else if (item.Name == "nItemID")
                        {
                            PlcWriteOpcHeaderData(item.TopicWrite, itmId);
                        }
                        else if (item.Name == "sTxTimeStamp")
                        {
                            PlcWriteOpcHeaderData(item.TopicWrite, TestDataHelper.GetTimeStamp());
                        }
                        else if (item.Name == "nItemStatus")
                        {
                            TestDataHelper.SetBitInShort(ref ItmStatus, 1, true);
                            PlcWriteOpcHeaderData(item.TopicWrite, ItmStatus);
                        }
                        else
                        {
                            Console.WriteLine("No item found");
                        }
                    }
                    stopwatch.Stop();

                    TimeSpan timeTaken = stopwatch.Elapsed;
                    Console.WriteLine($"Time Taken: {timeTaken}");
                }
            }
            else
            {
                Console.WriteLine("Session is not connected");
            }
        }
         private void PlcWriteOpcHeaderData<T>(string writeTopic, T value)
        {
            try
            {
                if (_session.Connected)
                {
                    NodeId nWrite = new NodeId(writeTopic);
                    DataValue writeValue = new DataValue();

                    writeValue.Value = new Variant(value);

                    _session.Write(null, new WriteValueCollection { new WriteValue { NodeId = nWrite, AttributeId = Attributes.Value, Value = writeValue } }, out status, out results);

                    if (status[0] != StatusCodes.Good)
                        Console.WriteLine($"Write Failed,Topic:{writeTopic} , value:{value}");

                }
                else
                {
                    Console.WriteLine("Session is not connected");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void PlcProcessHandle(string ReadTopicName, string WriteTopicName)
        {
            try
            {
                if (_session.Connected)
                {
                    NodeId nRead = new NodeId(ReadTopicName);
                    NodeId nWrite = new NodeId(WriteTopicName);

                    DataValue readValue = new DataValue();
                    DataValue writeValue = new DataValue();
                    
                    readValue = _session.ReadValue(nRead);
                    
                   
                    writeValue.Value = new Variant(readValue.Value);

                    _session.Write(null, new WriteValueCollection { new WriteValue { NodeId = nWrite, AttributeId = Attributes.Value, Value = writeValue } }, out status, out results);

                }
                else
                {
                    Console.WriteLine("Session is not connected");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
          
        }
        private bool GetPlcRequest( out short itmId,out byte ItmStatus)
        {

            try
            {
                NodeId nItemId = new NodeId("ns=4;s=Global.stOpcTxData.nItemID");
                NodeId nItemStatus = new NodeId("ns=4;Global.stOpcTxData.nItemStatus");
                // Read the value of the OPC nCheckNetwork
                DataValue itemId = new();
                DataValue itemStatus = new DataValue();
                itemId = _session.ReadValue(nItemId);
                itemStatus = _session.ReadValue(nItemStatus);

                itmId = (short)itemId.Value;
                ItmStatus = (byte)itemStatus.Value;
               
                if (ItmStatus == 1)
                    return true;
                else
                    return false;
            
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                itmId = 0;
                ItmStatus = 0;
                return false;
            }

        }
        public async Task NetworkCheck()
        {
            if (_session.Connected)
            {
                    NodeId nCheckNetwork = new NodeId("ns=4;s=Global.stOpcCheckNetwork.nCheckNetwork");

                try
                {
                    while(true)
                    {
                        
                        // Read the value of the OPC nCheckNetwork
                        checkNetworkValue = _session.ReadValue(nCheckNetwork);
                        echoCheckNetworkValue.Value = new Variant(checkNetworkValue.Value);
                        await _session.WriteAsync(null, new WriteValueCollection { new WriteValue { NodeId = new NodeId("ns=4;s=Global.stOpcCheckNetwork.nEchoCheckNetwork"), AttributeId = Attributes.Value, Value = echoCheckNetworkValue } }, CancellationToken.None);
                    }

                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
}