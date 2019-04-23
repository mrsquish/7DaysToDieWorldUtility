using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Model
{
    public class Neighbor : Vector2<int>
    {
        public Neighbor(int x, int z, double distance) : base(x, z)
        {
            Distance = distance;
        }

        public double Distance { get; set; }
    }
}
