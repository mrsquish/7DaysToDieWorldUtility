using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DaysToDie.Contracts;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Model
{
    public class RoadCell : Vector2<int>
    {
        public float Avg { get; set; }
       
        public Vector2<int> ToVector2(int cellSize)
        {
            return new Vector2<int>(X * cellSize, Z * cellSize);
        }

        public double CostFromOrigin { get; set; }
        public double DistanceFromOrigin { get; set; }

        public RoadCell Next { get; set; }
        public RoadCell Previous { get; set; }
    }
}
