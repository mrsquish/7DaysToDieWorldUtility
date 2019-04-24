using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Contracts
{
    public interface ITypedPathCell<T> : IVector2<int>
    {
        T Avg { get; set; }
        T CostToDestination { get; set; }
        T CostFromOrigin { get; set; }
        double DistanceFromOrigin { get; set; }
        bool ReachedMaximumCost(T maximum);

        void SetAverage(double avg);

        T CombineCost(T cost);

        bool HasLowerCost(T cost);
        
        ITypedPathCell<T> Next { get; set; }
        ITypedPathCell<T> Previous { get; set; }

        Vector2<int> ToVector2(int cellSize);
    }
}
