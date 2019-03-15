using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model
{
    public class Vector3Int : Vector3<int>
    {
        private Vector3<int> _zeroBasedVector3;

        public Vector3Int():base()
        {
        }

        public Vector3Int(int x, int y, int z) : base(x, y, z)
        {
        }

        public Vector3<int> ToZeroBased(WorldSize size)
        {
            return _zeroBasedVector3 ?? (_zeroBasedVector3 =
                       new Vector3<int>((size.Width / 2) + X, Y, (size.Height / 2) + Z));
        }
    }
}
