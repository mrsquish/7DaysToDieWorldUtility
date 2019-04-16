using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Model
{
    public class RoadCell
    {
        public float Max { get; set; }
        public float Min { get; set; }
        public float Avg { get; set; }
        public int AvgCellHeight { get; set; }
        public float G { get; set; }
        public float H { get; set; }

        public int X { get; set; }
        public int Z { get; set; }

        public Vector2<int> ToVector2(int cellSize)
        {
            return new Vector2<int>(X * cellSize, Z * cellSize);
        }
    }
}
