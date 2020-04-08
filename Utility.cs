using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using static simexercise.AppConfig;
using System;
using System.Net;

namespace simexercise
{

    public static class AppConfig {
        private static IConfigurationRoot _config;
        public static IConfigurationRoot Config { get => _config; set => _config = value; }
        public const string Prefix = "AZSIMULATOR_";

        public static void assertEnvVariable()
        {

            
            string[] env = {"MAPSKEY","IDSCOPE","REGISTRATIONID","SASTOKEN"};
            foreach (var v in env ) {
                var test = AppConfig.Config[v];
                if ( test == null ) {
                    Console.Error.WriteLine("Error {0} is not set.", Prefix + v);
                    Environment.Exit(1);
                }
            }
            var code = AtlasRoute.test();
            if (code.Equals(HttpStatusCode.Unauthorized))
            {            
                Console.Error.WriteLine("Error {0} is invalid, azure maps returned 401 unauthorized.", Prefix + AtlasRoute.AZMAPSKEY); 
                Environment.Exit(1);
            }
        }
    }

    public class DeviceRegistrationHelper
    {
        private const string provisioningHost = "global.azure-devices-provisioning.net";

        public static DeviceClient getDeviceClient()
        {           
            var idScope = Config["IDSCOPE"];
            var registrationId = Config["REGISTRATIONID"];
            var symmetricKey1 = Config["SASTOKEN"];

            var security = new SecurityProviderSymmetricKey(registrationId, symmetricKey1, null);
            using (var transport = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly))
            {
                var pClient = ProvisioningDeviceClient.Create(provisioningHost, idScope, security, transport);
                DeviceRegistrationResult result = pClient.RegisterAsync().GetAwaiter().GetResult();
                var s = $"HostName={result.AssignedHub};DeviceId={result.DeviceId};SharedAccessKey={security.GetPrimaryKey()}"; // result.AssignedHub , result.DeviceId, security.GetPrimaryKey();
                return DeviceClient.CreateFromConnectionString(s, TransportType.Mqtt);
            }
            
        }
    }
}