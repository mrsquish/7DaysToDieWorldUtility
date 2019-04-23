using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model
{
    public class RoadCellNeighbor
    {
        public RoadCell Cell { get; }
        public double DistanceFactor { get; }

        public RoadCellNeighbor(RoadCell cell, double distanceFactor)
        {
            Cell = cell;
            DistanceFactor = distanceFactor;
        }
    }
}
