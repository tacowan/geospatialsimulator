using System;

namespace simexercise
{
    public class GeoLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }


        public GeoLocation(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        public GeoLocation() { }

        public double DistanceFrom(GeoLocation loc2)
        {
            var R = 6371.01; // In kilometers
            var dLat = DegreesToRadians(loc2.Latitude - Latitude);
            var dLon = DegreesToRadians(loc2.Longitude - Longitude);
            double lat1 = DegreesToRadians(Latitude);
            double lat2 = DegreesToRadians(loc2.Latitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Asin(Math.Sqrt(a));
            return R * 2 * Math.Asin(Math.Sqrt(a));
        }

        public GeoLocation FindPointAtDistance(double initialBearingRadians, double distanceKilometres)
        {
            const double radiusEarthKilometres = 6371.01;
            var distRatio = distanceKilometres / radiusEarthKilometres;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);

            var startLatRad = DegreesToRadians(Latitude);
            var startLonRad = DegreesToRadians(Longitude);

            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);

            var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialBearingRadians)));

            var endLonRads = startLonRad
                + Math.Atan2(
                    Math.Sin(initialBearingRadians) * distRatioSine * startLatCos,
                    distRatioCosine - startLatSin * Math.Sin(endLatRads));

            return new GeoLocation()
            {
                Latitude = RadiansToDegrees(endLatRads),
                Longitude = RadiansToDegrees(endLonRads)
            };

        }

        public static double DegreesToRadians(double degrees)
        {
            const double degToRadFactor = Math.PI / 180;
            return degrees * degToRadFactor;
        }

        public static double RadiansToDegrees(double radians)
        {
            const double radToDegFactor = 180 / Math.PI;
            return radians * radToDegFactor;
        }


    }
}