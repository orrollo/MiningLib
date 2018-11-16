using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiningLib.Clustering;
using NUnit.Framework;

namespace MiningLib.Tests
{
    [TestFixture]
    public class MountainClusteringTests
    {
        [Test]
        public void Point1DTest()
        {
            var rnd = new Random();

            var list = new List<double>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(rnd.NormalDistribution(2, 1));
                list.Add(rnd.NormalDistribution(7, 1));
                list.Add(rnd.NormalDistribution(15, 1));
            }
            //list.Sort();

            //var dists = new List<double>();
            //for (int n1 = 0; n1 < 50; n1++)
            //{
            //    int p1 = rnd.Next(0, list.Count);
            //    int p2 = p1;
            //    while (p1 == p2) p2 = rnd.Next(0, list.Count);
            //    dists.Add(Math.Abs(list[p1] - list[p2]));
            //}

            //var alfa = 8.0 / dists.Average();

            var mnt = new MountainClustering<double>(src => new double[] { src });
            mnt.AddRange(list);
            var clusters = mnt.BuildClusters(10, null);

        }
    }
}
