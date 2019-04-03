using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Model
{
    public struct Vector2Int 
    {
        public Vector2Int(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int X { get; set; }
        public int Z { get; set; }


        public override string ToString()
        {
            return $"{X},{Z}";
        }
    }
}
