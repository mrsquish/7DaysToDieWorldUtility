using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model
{
    public class Vector3<T>
    {
        public Vector3()
        {
        }

        public Vector3(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }

        public override string ToString()
        {
            return $"{X},{Y},{Z}";
        }

    }
}
