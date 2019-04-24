using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DaysToDie.Contracts;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Model
{
    public class RoadCellLong : RoadCell<ulong>
    {
        public override bool ReachedMaximumCost(ulong maximum)
        {
            return base.CostFromOrigin >= maximum;
        }

        public override void SetAverage(double avg)
        {
            base.Avg = (ulong) avg;
        }

        public override ulong CombineCost(ulong cost)
        {
            return base.CostFromOrigin + cost;
        }

        public override bool HasLowerCost(ulong cost)
        {
            return base.CostFromOrigin <= cost;
        }
    }
}
