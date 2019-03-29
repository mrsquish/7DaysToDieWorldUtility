using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model
{
    public struct HeightMap2
    {
        private int _size;
        private ushort[] _heightMap;        

        public HeightMap2(int size)
        {
            _size = size;
            _heightMap = new ushort[size * size];
        }

        public int Size => _size;

        public ushort this[int x, int y]
        {
            get => _heightMap[x + (y * _size)];
            set => _heightMap[x + (y * _size)] = value;
        }
    }
}
