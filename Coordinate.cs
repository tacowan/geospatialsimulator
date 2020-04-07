using System;

namespace simexercise
{
    public enum GeoType { metertrace, linesegment, fullstop, begin, end }

    public interface RouteMarker
    {
        public GeoType Type { get; }

        public long TripMeters {get; set;}
        public void updateSimulationState(VehicleState state);

        public bool isEnd();
    }

    public struct LineSegmentStart : RouteMarker
    {
        

        public LineSegmentStart(LineSegment line) : this()
        {
            Latitude = line.begin.Latitude;
            Longitude = line.begin.Longitude;
            Type = GeoType.linesegment;
        }

        public GeoType Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public long TripMeters {get; set;}

        public bool isEnd()
        {
            return false;
        }

        public void updateSimulationState(VehicleState state)
        {
            int i = AtlasRoute.getSpeed(Latitude,Longitude).GetAwaiter().GetResult();
            if ( i == -1 )
                i = 10;
            state.maxSpeed = i;
            state.desiredSpeed = i;
        }
    }

    public struct Coordinate : RouteMarker
    {

        private static double EarthRadiusInMeters = 6371000.0;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public long TripMeters { get; set; }

        public GeoType Type { get; set; }



        public Coordinate(double latitude, double longitude, long tripmeters, GeoType t = GeoType.metertrace)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.TripMeters = tripmeters;
            this.Type = t;           
        }

        public Coordinate(Coordinate copy, GeoType t = GeoType.fullstop)
        {
            Latitude = copy.Latitude;
            Longitude = copy.Longitude;
            TripMeters = copy.TripMeters;
            Type = t;
        }
        
        // override object.Equals
        public override bool Equals(object obj)
        {            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Coordinate c = (Coordinate)obj;
            // TODO: write your implementation of Equals() here          
            return Latitude == c.Latitude && Longitude == c.Longitude;
        }

        
        public override String ToString()
        {
            return Latitude + "," + Longitude;
        }

        public bool isEnd()
        {
            return Type == GeoType.end;
        }

        public double DistanceFrom(Coordinate loc2)
        {
            var R = EarthRadiusInMeters; // In meters
            var dLat = DegreesToRadians(loc2.Latitude - Latitude);
            var dLon = DegreesToRadians(loc2.Longitude - Longitude);
            var lat1 = RLatitude();
            var lat2 = loc2.RLatitude();

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            return R * 2 * Math.Asin(Math.Sqrt(a));
        }

        public double RLatitude()
        {
            return DegreesToRadians(Latitude);
        }

        public double RLongitude()
        {
            return DegreesToRadians(Longitude);
        }

        public static double DegreesToRadians(double degrees)
        {
            const double degToRadFactor = Math.PI / 180;
            return degrees * degToRadFactor;
        }


        public static double GetAngle(double a, double b)
        {
            double d = 0;
            d = (b - a) % 360;
            if (d > 180)
                d -= 360;
            else if (d < -180)
                d += 360;
            return d;
        }
        public static double GetBearing(Coordinate originCoordinate, Coordinate destinationCoordinate)

        {
            return GetBearing(originCoordinate.Latitude, originCoordinate.Longitude, destinationCoordinate.Latitude,
                              destinationCoordinate.Longitude);

        }
        public static double GetBearing(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude)

        {
            var destinationRadian = (destinationLongitude - originLongitude).ToRadian();
            var destinationPhi = Math.Log(Math.Tan(destinationLatitude.ToRadian() / 2 + Math.PI / 4) / Math.Tan(originLatitude.ToRadian() / 2 + Math.PI / 4));

            if (Math.Abs(destinationRadian) > Math.PI)
                destinationRadian = destinationRadian > 0
                                        ? -(2 * Math.PI - destinationRadian)
                                        : (2 * Math.PI + destinationRadian);

            return Math.Atan2(destinationRadian, destinationPhi).ToBearing();
        }

        public void updateSimulationState(VehicleState s)
        {
            s.updateLocation(this);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude, Type);
        }
    }
}