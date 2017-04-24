using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Timers
{
    internal static class RandomExtensions
    {
        public static double Next(this Random random, double minValue, double maxValue)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            return ((maxValue - minValue) * random.NextDouble()) + minValue;
        }
    }
}
