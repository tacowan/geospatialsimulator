using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using static simexercise.DeviceRegistrationHelper;
using static simexercise.AppConfig;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace simexercise
{
    class Program
    {
        

        static int Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appconfig.json")
                .AddEnvironmentVariables(prefix: Prefix);

            var config = builder.Build();
            AppConfig.Config = config;
            assertEnvVariable();

            var s_deviceClient = getDeviceClient();
            begin(s_deviceClient).Wait();
            return 0;
        }

        static async Task begin(DeviceClient deviceClient)
        {
            // REST call to get azure maps rout data
            var json = await AtlasRoute.getRoute(32.747641, -97.324868, 32.748871, -97.3352362);
            var drivingRoute = new BlockingCollection<RouteMarker>(100);
            Task producer = new AtlasRoute(json, drivingRoute).GenerateMetersAsync();

            Task consumer = new Vehicle(drivingRoute).StartTrip(async (IoTState v) =>
            {
                var telemetryDataPoint = new
                {
                    Location = new { lon = v.Longitude, lat = v.Latitude },
                    Speed = v.Speed
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var msg = new Message(Encoding.UTF8.GetBytes(messageString));
                await deviceClient.SendEventAsync(msg);               
            }, 3);

            consumer.Wait();
        }
    }
}
