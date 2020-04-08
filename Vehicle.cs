using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace simexercise
{
    class Vehicle
    {
  
        private Coordinate nextStop = new Coordinate();
        Queue<IoTState> eventQueue;
        BlockingCollection<RouteMarker> route;

        decimal stoppedUntil = 0.0M;
   

        VehicleState s = new VehicleState()
        {
            maxSpeed = 10,
            acclerationRate = 2,
            acceleration = 0,
            Speed = 0
        };

        public Vehicle(BlockingCollection<RouteMarker> r)
        {
            eventQueue = new Queue<IoTState>();
            route = r;
        }


        public async Task StartTrip(Action<IoTState> a, int frequency = 2)
        {

            decimal t = 0.0M; // simulation time in seconds
            decimal dt = .1M; // step increment in seconds
            decimal position = 0; // position along route in meters
            bool done = false; // signal end of simulation
           
            Task task2 = Task.Run(async () =>
            {
                s.location = (Coordinate)route.Take();
                Debug.Assert(s.location.Type == GeoType.begin);
                while (!done)
                {
                    s.updateSpeed(dt);
                    // if we are stopped force speed to zero 
                    if (stoppedUntil >= 0 ) {
                        s.Speed = 0;
                        stoppedUntil -= dt;
                    }
                    position += s.Speed * dt;

                    done = move(position); //location may change
                    if (done)
                        break;

                    t += dt;
                    // simulation runs MUCH faster than real time, 
                    // vehicle sends location udpates
                    // once per frequency*seconds
                    if (t % frequency == 0)
                    {
                        // try and stay within a minute ahead of real time
                        if (eventQueue.Count > 100)
                            await Task.Delay(1000);
                        var state = new IoTState()
                        {
                            Latitude = s.location.Latitude,
                            Longitude = s.location.Longitude,
                            Speed = s.Speed,
                            Limit = s.maxSpeed
                        };
                        eventQueue.Enqueue(state);
                    }
                    s.adjustSpeed(nextStop);
                }
                // signal end to real time events
                eventQueue.Enqueue(
                    new IoTState() { Latitude = -1 }
                );
                Console.WriteLine("Simulation complete in {0}", TimeSpan.FromSeconds(Decimal.ToDouble(t)));
            });

            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(frequency * 1000);
                    if (eventQueue.Count < 1)
                        continue;
                    if (eventQueue.Peek().Latitude < 0)
                        break;
                    a.Invoke(eventQueue.Dequeue());
                }
                Console.WriteLine("realtime simulation complete");
            });

        }

        private bool move(decimal position)
        { 
            while (s.location.TripMeters <=  position)
            {
                var c=route.Take();
                c.updateSimulationState(s);
                if (c.isEnd())
                    return true;
                else if (c.Type == GeoType.fullstop) {
                    nextStop = (Coordinate)c;
                    //keep speed at zero for (n) seconds.  
                    stoppedUntil = 10;       
                }      
            }
            return false;
        }
    }
}