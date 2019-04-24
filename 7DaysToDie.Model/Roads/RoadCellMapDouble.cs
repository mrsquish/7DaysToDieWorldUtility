using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DaysToDie.Base;
using _7DaysToDie.Contracts;
using _7DaysToDie.Model;

namespace _7DaysToDie.Roads
{
    public class RoadCellMapDouble : RoadCellMap<double>
    {
        private static float _elevationFactor = 256;

        public RoadCellMapDouble(HeightMap<ushort> map, int resolution)
            : base(map, resolution, 
                () => double.MaxValue, 
                (d, d1) => d + d1,
                GetWeightedUnitDistance, 
                () => new DelegatedRoadCell<double>(MaximumReached, d => d, CombineCost, HasLowerCost))
        {

        }

        private static bool HasLowerCost(double nodeCost, double newCost)
        {
            return nodeCost <= newCost;
        }

        private static double CombineCost(double start, double cost)
        {
            return start + cost;
        }

        private static bool MaximumReached(double maximum, double cost)
        {
            return cost >= maximum;
        }

        private static double GetWeightedUnitDistance(ITypedPathCell<double> pointA, ITypedPathCell<double> pointB)
        {
            //var directLineDistance = GetDirectLineDistance(pointA, pointB);
            var heightDifference = Math.Abs(pointA.Avg - pointB.Avg);
            //return heightDifference;
            /*
            var weightedDifference = Math.Pow(2, Math.Min(32, heightDifference / 124));
            return weightedDifference;// * (directLineDistance / distanceFactor);
            */
            if (heightDifference < _elevationFactor)
                return heightDifference;

            var heightFactor = 1 + Math.Min(8, Math.Floor(heightDifference / 256));

            return (heightFactor * heightDifference);
        }
    }
}
