using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Base
{
    public static class WorldSettings
    {
        public const float SeaLevel = ((float)ushort.MaxValue / 255) * 35;
        public const float SeaFloor = ((float)ushort.MaxValue / 255) * 5;
        public const float GroundLevel = ((float)ushort.MaxValue / 255) * 37;

        public const float UnitLevel = ((float)ushort.MaxValue / 255);
    }
}
