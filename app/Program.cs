using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using static simexercise.DeviceRegistrationHelper;
using static simexercise.AppConfig;
using System.IO;
using Microsoft.Extensions.Configuration;

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

            double lat1, lon1;
            parseCoordinate("from", out lat1, out lon1);           
            double lat2, lon2;
            parseCoordinate("to", out lat2, out lon2);
            var s_deviceClient = getDeviceClient();
            Program p = new Program(Config);
            p.begin(s_deviceClient, lat1, lon1, lat2, lon2).Wait();
            return 0;
        }

        private static void parseCoordinate(string var, out double lat1, out double lon1)
        {
            var x = AppConfig.Config[var].Split(',');
            lat1 = double.Parse(x[0]);
            lon1 = double.Parse(x[1]);
        }

        Program(IConfiguration configuration) {
            config = configuration;
        }

        async Task begin(DeviceClient deviceClient, double lat1, double lon1, double lat2, double lon2)
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
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var msg = new Message(Encoding.UTF8.GetBytes(messageString));
                System.Console.WriteLine(messageString);
                await deviceClient.SendEventAsync(msg);               
            }, 5);

            t2.Wait();
        }
    }
}
