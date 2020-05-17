using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using static simexercise.AppConfig;
using System;

namespace simexercise
{

    public static class AppConfig {
        private static IConfigurationRoot _config;
        public static IConfigurationRoot Config { get => _config; set => _config = value; }
        public const string Prefix = "AZSIMULATOR_";

        public static void assertEnvVariable()
        {
           
            string[] env = {"MAPSKEY", "DEVICEID"};
            foreach (var v in env ) {
                var test = Config[v];
                if ( test == null ) {
                    Console.Error.WriteLine("Error {0} is not set.", Prefix + v);
                    Environment.Exit(1);
                }
            }
        }
    }
}