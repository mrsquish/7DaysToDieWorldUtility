using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model
{
    public class DelegatedRoadCell<T> : RoadCell<T>
    {
        private readonly Func<T, T, bool> _maximumReached;
        private readonly Func<double, T> _convertDouble;
        private readonly Func<T, T, T> _combineCost;
        private readonly Func<T, T, bool> _hasLowerCost;

        public DelegatedRoadCell(Func<T, T, bool> maximumReached, Func<double, T> convertDouble,
            Func<T,T, T> combineCost, Func<T, T, bool> hasLowerCost)
        {
            _maximumReached = maximumReached;
            _convertDouble = convertDouble;
            _combineCost = combineCost;
            _hasLowerCost = hasLowerCost;
        }

        public override bool ReachedMaximumCost(T maximum)
        {
            return _maximumReached(maximum, base.CostFromOrigin);
        }

        public override void SetAverage(double avg)
        {
            base.Avg = _convertDouble(avg);
        }

        public override T CombineCost(T cost)
        {
            return _combineCost(base.CostFromOrigin, cost);
        }

        public override bool HasLowerCost(T cost)
        {
            return _hasLowerCost(base.CostFromOrigin, cost);
        }
    }
}
