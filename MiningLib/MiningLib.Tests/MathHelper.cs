using System;

namespace MiningLib.Tests
{
    public static class MathHelper
    {
        public static double NormalDistribution(this Random rand, double mean, double stdDev)
        {
            while (true)
            {
                var u = 2.0 * rand.NextDouble() - 1;
                var v = 2.0 * rand.NextDouble() - 1;
                var s = u * u + v * v;
                if (s < 1.0 && s > 0.0)
                {
                    double t = u * Math.Sqrt(-2.0 * Math.Log(s) / s);
                    return mean + t * stdDev;
                }
            }
        }
    }
}