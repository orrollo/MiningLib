using System;
using System.Linq;

namespace MiningLib.Common
{
    public static class DistanceHelper
    {
        public static double EuclidDistance(double[] one, double[] other)
        {
            double result = 0.0;
            if (one.Length > 0 && one.Length == other.Length)
            {
                var sum = one.Select((v, i) => { var dv = v - other[i]; return dv * dv; }).Sum();
                result = Math.Sqrt(sum);
            }
            return result;
        }
    }
}