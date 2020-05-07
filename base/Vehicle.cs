using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace simexercise
{
    public class Vehicle
    {

        private Coordinate nextStop = new Coordinate();
        Queue<IoTState> eventQueue;
        //BlockingCollection<RouteMarker> route;
        decimal stoppedUntil = 0.0M;
        VehicleState s;
        AtlasRoute _route;
        bool _realtime;

        public Vehicle(AtlasRoute route, bool realtime = true)
        {
            _route = route;
            _realtime = realtime;

            eventQueue = new Queue<IoTState>();
            
            s = new VehicleState()
            {
                maxSpeed = 10,
                acclerationRate = 2,
                acceleration = 0,
                Speed = 0
            };
        }


        public async Task StartTrip(Action<IoTState> a, int frequency = 2)
        {

            decimal t = 0.0M; // simulation time in seconds
            decimal dt = .1M; // step increment in seconds
            decimal position = 0; // position along route in meters
            bool done = false; // signal end of simulation

            Task task2 = Task.Run(async () =>
            {
                s.location = (Coordinate)_route.Take();
                Debug.Assert(s.location.Type == GeoType.begin);
                while (!done)
                {
                    s.updateSpeed(dt);
                    // if we are stopped force speed to zero 
                    if (stoppedUntil >= 0)
                    {
                        s.Speed = 0;
                        stoppedUntil -= dt;
                    }
                    position += s.Speed * dt;
                    done = move(position); //location may change
                    if (done)
                        break;

                    t += dt; //increment time

                    // simulation runs MUCH faster than real time, 
                    // vehicle sends location updates
                    // once per frequency*seconds
                    if (t % frequency == 0)
                    {
                        // try and stay within a minute ahead of real time
                        if (eventQueue.Count > 100)
                            await Task.Delay(1000);                     
                        eventQueue.Enqueue(new IoTState(s,t));
                    }
                    s.adjustSpeed(nextStop);
                }
                // signal end to real time events
                eventQueue.Enqueue(
                    new IoTState() { Latitude = -1 }
                );
                Console.WriteLine("Virtual simulation completed, duration: {0}", TimeSpan.FromSeconds(Decimal.ToDouble(t)));
            });

            await Task.Run(async () =>
            {
                while (true)
                {
                    //await Task.Delay(frequency * 1000);
                    if (_realtime) await Task.Delay(frequency * 1000);
                    if (eventQueue.Count < 1)
                        continue;
                    if (eventQueue.Peek().Latitude < 0)
                        break;
                    a.Invoke(eventQueue.Dequeue());
                }
            });

        }

        public void updateSimulation(LineSegmentStart coord)
        {
            int i = _route.getSpeed(coord.Latitude,coord.Longitude).GetAwaiter().GetResult();
            //convert kph to meters per second
            if ( i == -1 )
                i = 10;
            var result = i*0.27777778;
            s.maxSpeed = (decimal)result;
        }

        internal void updateSimulation(Coordinate coordinate)
        {
            if (coordinate.Type  != GeoType.fullstop)
                s.updateLocation(coordinate);
        }
        
        private bool move(decimal position)
        {
            while (s.location.TripMeters <= position)
            {
                var c = _route.Take();
                c.updateSimulationState(this);
                if (c.isEnd())
                    return true;
                else if (c.Type == GeoType.fullstop)
                {
                    nextStop = (Coordinate)c;
                    //keep speed at zero for (n) seconds.  
                    stoppedUntil = 10;
                }
            }
            return false;
        }


    }
}