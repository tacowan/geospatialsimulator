using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace simexercise
{
    internal class BaseRoute
    {
        protected Coordinate[] items;
        protected LineSegment[] segments;
        BlockingCollection<RouteMarker> Breadcrumbs;

        protected long tripmeters;
   
        public BaseRoute(BlockingCollection<RouteMarker> r) {
            Breadcrumbs = r;
            tripmeters = 0;
        }

        public async Task GenerateMetersAsync() {
            await Task.Run( () => GenerateMeters());
        }

        public void GenerateMeters()
        {
            var begin = items[0];
            //this is the starting point and expected to be the 1st element
            Breadcrumbs.Add(new Coordinate(begin, GeoType.begin));

            // create line segment pairs and all
            // hard stops (right angles)
            // unfortunately no specific traffic light/stop sign
            // data is provided by azure maps JSON.
            segments = new LineSegment[items.Length - 1];
            Queue<Coordinate> stops = new Queue<Coordinate>();
            for (var i = 1; i < items.Length; i++)
            {
                var end = items[i];
                segments[i - 1] = new LineSegment(begin, end);
                if (i == 1) continue;
                double angle = segments[i - 1].GetAngle(segments[i - 2]);
                angle = Math.Abs(angle);
                //mark as having a full stop
                if (angle > 60 && angle < 120)
                {
                    stops.Enqueue(new Coordinate(begin));
                }
                begin = end;
            }
            // the final stop
            stops.Enqueue(new Coordinate(items[items.Length-1]));

            Coordinate nextStop = stops.Dequeue();
            Breadcrumbs.Add(nextStop);                
            
            // divide the segments into locations (meters) tracing the route.
            for (var i = 0; i < segments.Length; i++)
            {
                LineSegment line = segments[i];
                if (line.begin.Equals(nextStop)) {
                    nextStop = stops.Dequeue();
                    Breadcrumbs.Add(nextStop);      
                }                
                Breadcrumbs.Add(new LineSegmentStart(line));            
                foreach (var midpoint in Slice(segments[i]))
                    Breadcrumbs.Add(midpoint);
            }
            Breadcrumbs.Add(new Coordinate(items[items.Length-1], GeoType.end));   
        }

        public RouteMarker consumeEvent()
        {
            return Breadcrumbs.Take();
        }

        private Coordinate[] Slice(LineSegment line)
        {
            int parts = (int)Math.Round(line.Length());

            Coordinate to = line.end;
            Coordinate from = line.begin;
            double latdiff = to.Latitude - from.Latitude;
            double londiff = to.Longitude - from.Longitude;

            double latincrement = latdiff / parts;
            double lonincrement = londiff / parts;
            Coordinate[] result = new Coordinate[parts];
            result[0] = from;
            result[0].TripMeters = tripmeters++;
            for (int i = 1; i < parts; i++)
            {
                double lat = from.Latitude + (latincrement * i);
                double lon = from.Longitude + (lonincrement * i);
                result[i] = new Coordinate(lat, lon, tripmeters++);
            }
            return result;
        }
    }
}