using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Model
{
    public class Vector2<T>
    {
        public Vector2()
        {
        }

        public Vector2(T x, T z)
        {
            X = x;
            Z = z;            
        }

        public T X { get; set; }
        public T Z { get; set; }
        

        public override string ToString()
        {
            return $"{X},{Z}";
        }

    }
}
