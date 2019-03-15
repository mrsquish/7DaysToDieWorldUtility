using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model
{
    public class WorldSize
    {
        public int Height { get; set; }
        public int Width { get; set; }

        internal WorldSize()
        {
        }

        public static WorldSize WorldSize4()
        {
            return new WorldSize() {Height = 4096, Width = 4096};
        }

        public static WorldSize WorldSize8()
        {
            return new WorldSize() { Height = 8192, Width = 8192 };
        }

        public static WorldSize WorldSize16()
        {
            return new WorldSize() { Height = 16384, Width = 16384 };
        }

        public static WorldSize FromImageHeight(uint height)
        {
            if (height == 4096)
                return WorldSize4();
            if (height == 8192)
                return WorldSize8();
            if (height == 16384)
                return WorldSize16();
            throw new ArgumentException($"{nameof(height)} argument must be either 4096, 8192 or 16384");
        }
    }
}
