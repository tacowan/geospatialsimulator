namespace simexercise
{

    public struct LineSegment
    {
        public Coordinate begin {get;set;}
        public Coordinate end {get; set;}
        public GeoType type {get;set;}

        public LineSegment(Coordinate a, Coordinate b) {
            begin = a;
            end = b;  
            type = GeoType.metertrace;      
        }

        public double Length() {
            return begin.DistanceFrom(end);
        }

        public double GetBearing() {
            return Coordinate.GetBearing(begin, end);
        }

        public double GetAngle( LineSegment line) {
            return Coordinate.GetAngle(GetBearing(), line.GetBearing());
        }
    }
}
