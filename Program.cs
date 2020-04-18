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
                .AddCommandLine(args)
                .AddEnvironmentVariables(prefix: Prefix);


            var config = builder.Build();
            AppConfig.Config = config;
            assertEnvVariable();

            double lat1, lon1;
            parseCoordinate("from", out lat1, out lon1);           
            double lat2, lon2;
            parseCoordinate("to", out lat2, out lon2);
            var s_deviceClient = getDeviceClient();
            
            begin(s_deviceClient, lat1, lon1, lat2, lon2).Wait();
            return 0;
        }

        private static void parseCoordinate(string var, out double lat1, out double lon1)
        {
            var x = AppConfig.Config[var].Split(',');
            lat1 = double.Parse(x[0]);
            lon1 = double.Parse(x[1]);
        }

        static async Task begin(DeviceClient deviceClient, double lat1, double lon1, double lat2, double lon2)
        {
            
            // REST call to get azure maps route data
            var json = await AtlasRoute.getRoute(lat1, lon1, lat2, lon2);
            var drivingRoute = new BlockingCollection<RouteMarker>(100);

            // begin reading route line segments, could take a long time
            // so we will do this in parallel with the simulation.
            Task producer = new AtlasRoute(json, drivingRoute).GenerateMetersAsync();

            Task consumer = new Vehicle(drivingRoute).StartTrip(async (IoTState v) =>
            {
                var telemetryDataPoint = new
                {
                    Location = new { lon = v.Longitude, lat = v.Latitude },
                    Speed = v.Speed * 3.6M
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var msg = new Message(Encoding.UTF8.GetBytes(messageString));
                System.Console.WriteLine(messageString);
                await deviceClient.SendEventAsync(msg);               
            }, 5);

            consumer.Wait();
        }
    }
}
