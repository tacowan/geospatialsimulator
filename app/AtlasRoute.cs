using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using static System.Environment;
using static simexercise.AppConfig;

namespace simexercise
{
    class AtlasRoute : BaseRoute
    {
 
        public const string AZMAPSKEY = "MAPSKEY";
        public AtlasRoute(string json, BlockingCollection<RouteMarker> r) : base(r)
        {           
            Parse(json);
        }

        private void Parse(string json)
        { 
            dynamic rss = JObject.Parse(json);           
            var itineraryItems = rss["routes"][0]["legs"][0]["points"];
            List<Coordinate> list = new List<Coordinate>();
            foreach (dynamic c in itineraryItems)
            {
                var newc = new Coordinate
                {
                    Latitude = c.latitude,
                    Longitude = c.longitude
                };
                list.Add(newc);
            }
            items = list.ToArray();
        }

        public static HttpStatusCode  test() {
            string URL = "https://atlas.microsoft.com/timezone/ianaVersion/json";
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(URL)
            };
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["api-version"] = "1.0";   
            query["subscription-key"] = Config[AZMAPSKEY];
            return client.GetAsync("?" + query).GetAwaiter().GetResult().StatusCode;          
 
        }

        public static async Task<string> getRoute(double lat, double lon, double lat2, double lon2) {
            string URL = "https://atlas.microsoft.com/route/directions/json";
            ;
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(URL)
            };
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["api-version"] = "1.0";   
            query["subscription-key"] = Config[AZMAPSKEY];
            query["query"] = $"{lat},{lon}:{lat2},{lon2}";
            string queryString = query.ToString();      
            return await client.GetStringAsync("?" + queryString);            
        }

        public static async Task<int> getSpeed(double lat, double lon) {
            
            var URL = "https://atlas.microsoft.com/traffic/flow/segment/json";
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(URL)
            };
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["api-version"] = "1.0";
            query["subscription-key"] = Config[AZMAPSKEY];
            query["query"] = $"{lat},{lon}";
            query["zoom"] = "21";
            query["style"] = "relative";
            string queryString = query.ToString();
            try {
                var s = await client.GetStringAsync("?" + queryString); 
                JObject rss = JObject.Parse(s);
                var currentSpeed = rss["flowSegmentData"]; 
                var i = currentSpeed.Value<int>("currentSpeed");
                return i;         
            } catch (HttpRequestException) {
                return -1;
            }
        }
    }
}
