using System;

namespace simexercise
{
    /// <summary>
    /// Extension methods used by Geolocation.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Gets the radian.
        /// </summary>
        /// <param name="d">The double.</param>
        /// <returns>Double.</returns>
        public static double ToRadian(this double d)
        {
            return d * (Math.PI / 180);
        }

 
        /// <summary>
        /// Gets the degrees.
        /// </summary>
        /// <param name="r">The radian.</param>
        /// <returns>Double.</returns>
        public static double ToDegrees(this double r)
        {
            return r * 180 / Math.PI;
        }

        /// <summary>
        /// Gets the bearing.
        /// </summary>
        /// <param name="r">The radian.</param>
        /// <returns>Double.</returns>
        public static double ToBearing(this double r)
        {
            double degrees = ToDegrees(r);
            return (degrees + 360) % 360;
        }
    }
}