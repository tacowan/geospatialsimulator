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
        /// Diffs the radian.
        /// </summary>
        /// <param name="val1">First value.</param>
        /// <param name="val2">Second value.</param>
        /// <returns>Double.</returns>
        public static double DiffRadian(this double val1, double val2)
        {
            return val2.ToRadian() - val1.ToRadian();
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