using System;
using System.Collections.Generic;
using System.Linq;
using MiningLib.Common;

namespace MiningLib.Clustering
{
    public class MountainClustering<T>
    {
        protected ToVectorDelegate<T> _toVector;
        protected Dictionary<T,double[]> vectors = new Dictionary<T, double[]>();
        protected List<double> minValues = new List<double>(), maxValues = new List<double>();

        public MountainClustering(ToVectorDelegate<T> converter)
        {
            ToVector = converter ?? throw new ArgumentException();
        }

        public void AddItem(T entity)
        {
            if (vectors.ContainsKey(entity)) return;
            var vector = ToVector(entity);
            vectors[entity] = vector;
            UpdateRanges(vector);
        }

        public void AddRange(IEnumerable<T> src)
        {
            foreach (var item in src) AddItem(item);
        }

        private void UpdateRanges(double[] vector)
        {
            for (int index = 0; index < vector.Length; index++)
            {
                double value = vector[index];
                if (index >= minValues.Count)
                {
                    minValues.Add(double.MaxValue);
                    maxValues.Add(double.MinValue);
                }

                if (minValues[index] > value) minValues[index] = value;
                if (maxValues[index] < value) maxValues[index] = value;
            }
        }

        public Dictionary<T, double> BuildClusters(int intervalsNumber = 10, DistanceDelegate distance = null, double alfa = -1.0, double beta = -1.0)
        {
            if (distance == null) distance = DistanceHelper.EuclidDistance;
            var axes = BuildAxes(intervalsNumber);

            var centers = new List<double[]>();
            var current = new double[axes.Count];
            RecursiveBuildCenters(axes, centers, ref current, 0);

            if (alfa <= 0.0) alfa = AproximateAlfa(distance);
            if (beta <= 0.0) beta = alfa * 2.25;

            var potentias = CalculatePotentias(distance, alfa, centers);

            // вычисляем вершины по очереди

            var xcenters = OrderCentersByPotentias(potentias, distance, beta);
            var result = FindClosestPoints(xcenters, distance);

            return result;
        }

        private double AproximateAlfa(DistanceDelegate distance)
        {
            var rnd = new Random();

            var vc = vectors.Count;
            var totalPairs = (vc * (vc - 1)) >> 1;
            var number = Math.Min(50, totalPairs);

            var dists = 0.0;

            var items = vectors.Keys.ToArray();
            for (int n1 = 0; n1 < number; n1++)
            {
                int index1 = rnd.Next(0, vc);
                int index2 = index1;
                while (index1 == index2) index2 = rnd.Next(0, vc);
                T item1 = items[index1], item2 = items[index2];
                dists += distance(vectors[item1], vectors[item2]);
            }

            return dists / number;
        }

        private Dictionary<T, double> FindClosestPoints(Dictionary<double[], double> xcenters, DistanceDelegate distance)
        {
            var result = new Dictionary<T, double>();
            foreach (var center in xcenters)
            {
                var isFirst = true;
                var bestDist = 0.0;
                T bestMatch = default(T);
                foreach (var vector in vectors)
                {
                    var dist = distance(center.Key, vector.Value);
                    if (!isFirst && bestDist < dist) continue;
                    if (result.ContainsKey(vector.Key)) continue;
                    bestMatch = vector.Key;
                    bestDist = dist;
                    isFirst = false;
                }

                if (isFirst) continue;
                result[bestMatch] = center.Value;
            }

            return result;
        }

        private static Dictionary<double[], double> OrderCentersByPotentias(Dictionary<double[], double> potentias,
            DistanceDelegate distance, double beta)
        {
            var results = new Dictionary<double[], double>();
            var centers = potentias.Keys.ToList();
            while (centers.Count > 0)
            {
                centers.Sort((a, b) => potentias[b].CompareTo(potentias[a]));
                var best = centers.First();
                var bestPot = potentias[best];

                results[best] = bestPot;
                potentias.Remove(best);
                centers.Remove(best);
                foreach (var center in centers)
                {
                    var dist = distance(best, center);
                    potentias[center] -= bestPot * Math.Exp(-beta * dist);
                }
            }
            return results;
        }

        private Dictionary<double[], double> CalculatePotentias(DistanceDelegate distance, double alfa, List<double[]> centers)
        {
            Dictionary<double[], double> potentias = new Dictionary<double[], double>();
            foreach (var center in centers)
            {
                var pot = 0.0;
                foreach (var vector in vectors)
                {
                    var dist = distance(center, vector.Value);
                    pot += Math.Exp(-alfa * dist);
                }

                potentias[center] = pot;
            }

            return potentias;
        }

        private void RecursiveBuildCenters(List<List<double>> axes, List<double[]> centers, ref double[] current, int axeNumber)
        {
            if (axeNumber >= axes.Count)
            {
                var newCenter = current.Select(x => x).ToArray();
                centers.Add(newCenter);
                return;
            }
            var axe = axes[axeNumber];
            for (int idx = 0; idx < axe.Count; idx++)
            {
                current[axeNumber] = axe[idx];
                RecursiveBuildCenters(axes, centers, ref current, axeNumber + 1);
            }
        }

        protected const double epsilon = double.Epsilon * 1024;

        private List<List<double>> BuildAxes(int intervalsNumber)
        {
            intervalsNumber = Math.Max(1, intervalsNumber);
            var result = new List<List<double>>();
            for (int index = 0; index < minValues.Count; index++)
            {
                var max = maxValues[index];
                var min = minValues[index];
                var step = Math.Max(max - min, epsilon) / intervalsNumber;
                var shift = step / 8;
                min -= 3*shift;
                var axeMarks = new List<double>();
                double value = min;
                for (int num = 0; num <= intervalsNumber; num++)
                {
                    axeMarks.Add(value);
                    value += step;
                }
                result.Add(axeMarks);
            }
            return result;
        }

        public ToVectorDelegate<T> ToVector
        {
            get { return _toVector; }
            protected set { _toVector = value; }
        }
    }
}