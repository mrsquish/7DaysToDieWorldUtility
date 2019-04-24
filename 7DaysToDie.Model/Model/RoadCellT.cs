using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DaysToDie.Contracts;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Model
{
    public abstract class RoadCell<T> : Vector2<int>, ITypedPathCell<T>
    {
        public T Avg { get; set; }
       
        public Vector2<int> ToVector2(int cellSize)
        {
            return new Vector2<int>(X * cellSize, Z * cellSize);
        }

        public T CostToDestination { get; set; }

        public T CostFromOrigin { get; set; }
        public double DistanceFromOrigin { get; set; }

        public abstract bool ReachedMaximumCost(T maximum);
        public abstract void SetAverage(double avg);

        public abstract T CombineCost(T cost);
        public abstract bool HasLowerCost(T cost);
        
        public ITypedPathCell<T> Next { get; set; }

        public ITypedPathCell<T> Previous { get; set; }
    }
}
