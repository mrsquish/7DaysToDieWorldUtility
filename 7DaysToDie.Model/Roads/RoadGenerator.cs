using _7DaysToDie.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Roads
{
    public class RoadGenerator
    {
        private readonly int _size;
        private readonly HeightMap _heightMap;
        private readonly RgbBitMap _map;

        private int cellSize = 8;

        private float[,] cellAverages;

        public RoadGenerator(int size, HeightMap heightMap)
        {
            _size = size;
            _heightMap = heightMap;
            _map = new RgbBitMap(_size);
        }

        public void Generate()
        {
            GenerateCellAverageMap();
        }

        private void GenerateCellAverageMap()
        {
            cellAverages = new float[_size/cellSize, _size/cellSize];
            for (int z = cellSize; z < _size; z+= cellSize)
            {
                for (int x = cellSize; x < _size; x+=cellSize)
                {
                    cellAverages[x, z] = CellAverageHeight(x,z);
                }
            }
        }
        public float CellAverageHeight(int x, int z)
        {
            return (
                       _heightMap[x - cellSize, z] +
                       _heightMap[x - cellSize, z - cellSize] +
                       _heightMap[x, z - cellSize] +
                       _heightMap[x, z]) / 4;
        }

        public void Save(string filePath)
        {
            _map.Save(Path.Combine(filePath, "splat3.png"));
        }
    }
}
