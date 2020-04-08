using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Net;
using System;
using System.Collections.Concurrent;
using static System.Console;
using static simexercise.DeviceRegistrationHelper;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace simexercise
{
    class Program
    {
        private const string Prefix = "AZSIMULATOR_";

        static int Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables(prefix: Prefix);

            var config = builder.Build();
            AppConfig.Config = config;
            assertEnvVariable();
         
 
            var s_deviceClient = getDeviceClient();
            begin(s_deviceClient).Wait();
            return 0;
        }

        private static void assertEnvVariable()
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


        static async Task begin(DeviceClient deviceClient)
        {

            var foo = await AtlasRoute.getRoute(32.747641, -97.324868, 32.748871, -97.3352362);
            var drivingRoute = new BlockingCollection<RouteMarker>(100);
            Task producer = new AtlasRoute(foo, drivingRoute).GenerateMetersAsync();

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
