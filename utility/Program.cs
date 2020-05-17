using System.Threading.Tasks;
using static simexercise.AppConfig;
using System.IO;
using Microsoft.Extensions.Configuration;
using Azure.Messaging.EventHubs.Producer;

namespace simexercise
{
    class Program
    {
        IConfiguration config;
        int rowkey=0;
        static int Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddCommandLine(args)
                .AddEnvironmentVariables(prefix: Prefix);

            Config = builder.Build();
            assertEnvVariable();

            var waypoints = parseWaypoints("waypoints");
            //var s_deviceClient = getDeviceClient();
            Program p = new Program(Config);
            System.Console.WriteLine("PartitionKey,RowKey,t,Latitude,Longitude,SpeedKPH");
            for(int i=1; i<waypoints.Length/2; i++) {
                var lat1 = waypoints[i-1,0];
                var lon1 = waypoints[i-1,1];
                var lat2 = waypoints[i,0];
                var lon2 = waypoints[i,1];
                p.begin(lat1, lon1, lat2, lon2).Wait();
            }
            return 0;
        }

        private static double[,] parseWaypoints(string var)
        {
            var x = Config[var].Split(':');
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

        async Task begin(double lat1, double lon1, double lat2, double lon2)
        {

            var producer = new AtlasRoute(config);
            var json = await producer.getRoute(lat1, lon1, lat2, lon2);
            producer.Parse(json);

            Task t1 = producer.GenerateMetersAsync();
            var v = new Vehicle(producer, false);

            var deviceId = Config["DEVICEID"];
            Task t2 = v.StartTrip( (IoTState v) =>
            {
                var telemetryDataPoint = new
                {
                    Location = new { lon = v.Longitude, lat = v.Latitude },
                    Speed = v.Speed * 3.6M
                };
                System.Console.WriteLine($"{deviceId},{rowkey++},{v.T},{v.Latitude},{v.Longitude},{v.Speed * 3.6M}");
            }, 5);
            t2.Wait();
        }
    }
}
