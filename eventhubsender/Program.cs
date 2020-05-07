using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static simexercise.DeviceRegistrationHelper;
using static simexercise.AppConfig;
using System.IO;
using Microsoft.Extensions.Configuration;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs;

namespace simexercise
{
    class Program
    {
        IConfiguration config;
        static int Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddCommandLine(args)
                .AddEnvironmentVariables(prefix: Prefix);

            Config = builder.Build();
            assertEnvVariable();

            var waypoints = parseWaypoints("waypoints");
            var s_deviceClient = getDeviceClient();
            Program p = new Program(Config);

            for(int i=1; i<waypoints.Length; i++) {
                var lat1 = waypoints[i-1,0];
                var lon1 = waypoints[i-1,1];
                var lat2 = waypoints[i,0];
                var lon2 = waypoints[i,1];
                p.begin(s_deviceClient, lat1, lon1, lat2, lon2).Wait();
            }
            return 0;
        }

        private static void parseCoordinate(string var, out double lat1, out double lon1)
        {
            var x = AppConfig.Config[var].Split(',');
            lat1 = double.Parse(x[0]);
            lon1 = double.Parse(x[1]);
        }

        private static double[,] parseWaypoints(string var)
        {
            var x = AppConfig.Config[var].Split(':');

            var waypoints = new double[x.Length, 2];

            for (int i=0; i<x.Length; i++)
            {            
                var y = x[i].Split(',');    
                var lat1 = double.Parse(y[0]);
                var lon1 = double.Parse(y[1]);
                waypoints[i,0] = lat1;
                waypoints[i,1] = lon1;
            }
            return waypoints;
        }

        Program(IConfiguration configuration)
        {
            config = configuration;
        }

        async Task begin(EventHubProducerClient client, double lat1, double lon1, double lat2, double lon2)
        {

            var producer = new AtlasRoute(config);
            var json = await producer.getRoute(lat1, lon1, lat2, lon2);
            producer.Parse(json);

            Task t1 = producer.GenerateMetersAsync();
            var v = new Vehicle(producer);

            Task t2 = v.StartTrip(async (IoTState v) =>
            {
                var telemetryDataPoint = new
                {
                    Location = new { lon = v.Longitude, lat = v.Latitude },
                    Speed = v.Speed * 3.6M
                };
                System.Console.WriteLine($"{},{},{}"); 
 /*              var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                using EventDataBatch eventBatch = await client.CreateBatchAsync();
                var eventnew = new EventData(Encoding.UTF8.GetBytes(messageString));
                eventnew.Properties["deviceid"] = "foo";
                eventBatch.TryAdd(eventnew);
 */
            }, 0);

            t2.Wait();
        }


    }
}
