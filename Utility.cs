using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using static simexercise.AppConfig;

namespace simexercise
{

    public static class AppConfig {
        private static IConfigurationRoot _config;
        public static IConfigurationRoot Config { get => _config; set => _config = value; }
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