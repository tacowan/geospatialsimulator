using System;

namespace simexercise
{
    struct IoTState
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal Speed { get; set; }
        public decimal Limit { get; internal set; }
    }

    public class VehicleState
    {
        public decimal acceleration { get; set; }
        public decimal acclerationRate { get; set; }

        public decimal Speed { get; set; }
        public decimal maxSpeed { get; internal set; }

        public Coordinate location { get; set; }

        public decimal getStoppingDistance()
        {
            return (Speed / acclerationRate) * (Speed / 2);
        }

        internal void updateSpeed(decimal dt)
        {
            Speed += (acceleration * dt);
            
            if (Speed < 0)
            {
                Speed = .2M;
            }
        }

        internal void updateLocation(Coordinate c)
        {
            location = c;
        }

        internal void adjustSpeed(Coordinate nextStop)
        {
            var stopSignDistance = (decimal)location.DistanceFrom(nextStop);
            //Console.WriteLine("v={0:0.00}m/s {1:0.00} should be < {2:0.00}", Speed, getStoppingDistance(), stopSignDistance);
            if (getStoppingDistance() >= Math.Floor(stopSignDistance)) {
                //ignore all else and start slowing down
                acceleration = -acclerationRate;            
            } 
            else if (Speed < maxSpeed) //accelerate
            {
                acceleration = acclerationRate;
            }
            else if (Speed > maxSpeed) // declerate
            {
                acceleration = -acclerationRate;
            }
            
        }
    }



}
