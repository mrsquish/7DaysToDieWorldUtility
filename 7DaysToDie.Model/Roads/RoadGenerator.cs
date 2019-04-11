using _7DaysToDie.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace _7DaysToDie.Roads
{
    

    public class RoadGenerator
    {
        private readonly int _size;
        private readonly int _maxPoiCount;
        private readonly _7DaysToDie.Model.HeightMap _heightMap;
        private readonly RgbBitMap _map;

        private int cellSize = 8;

        private RoadCell[,] cellAverages;
        private List<Vector2i> _poiPossibleLocations = new List<Vector2i>();
        private List<Vector2i> _poiLocations = new List<Vector2i>();
        private Random _random;

        public RoadGenerator(int size, int maxPoiCount , _7DaysToDie.Model.HeightMap heightMap)
        {
            _size = size;
            _maxPoiCount = maxPoiCount;
            _heightMap = heightMap;
            _map = new RgbBitMap(_size);
            _random = new Random(DateTime.Now.Millisecond + (DateTime.Now.Minute <<  9));
        }

        public void Generate()
        {
            GenerateCellMetricsMap();
            FindPoiLocations();
            GenerateRoadsBetweenPois();
        }

        private void GenerateRoadsBetweenPois()
        {
            for (int poiIndex = 0; poiIndex < _poiLocations.Count-2; poiIndex++)
            {
                GenerateRoad(_poiLocations[poiIndex], _poiLocations[poiIndex + 1]);
            }
        }

        private void GenerateRoad(Vector2i pointA, Vector2i pointB)
        {

        }

        private void FindPoiLocations()
        {            
            while (_poiPossibleLocations.Count > 0 && 
                   _poiLocations.Count < _poiPossibleLocations.Count &&
                   _poiLocations.Count < _maxPoiCount)
            {
                _poiLocations.Add(GetRandomPoiLocation());
            }
        }

        private Vector2i GetRandomPoiLocation()
        {
            var location = _poiPossibleLocations[_random.Next(0, _poiPossibleLocations.Count - 1)];
            while (_poiLocations.Contains(location))
            {
                location = _poiPossibleLocations[_random.Next(0, _poiPossibleLocations.Count - 1)];
            }
            return location;
        }

        private void GenerateCellMetricsMap()
        {
            cellAverages = new RoadCell[_size/cellSize, _size/cellSize];
            CellLoop((x, z) => cellAverages[x, z] = GetCellMetrics(x, z));
        }

        private void CellLoop(Action<int, int> cellAction)
        {
            for (int z = 0; z < (_size/cellSize); z++)
            {
                for (int x = 0; x < (_size / cellSize); x++)
                {
                    cellAction(x, z);                    
                }
            }
        }

        public RoadCell GetCellMetrics(int x, int z)
        {
            var metrics = new RoadCell()
            {
                Avg = (
                        _heightMap[x + cellSize, z] +
                        _heightMap[x + cellSize, z + cellSize] +
                        _heightMap[x, z + cellSize] +
                        _heightMap[x, z]) / 4,
                Max = 0,
                Min = ushort.MaxValue
            };
            for (int i = z; i <= (z - cellSize); i++)
            {
                for (int j = x; j < (x - cellSize); j++)
                {
                    if (_heightMap[i, j] > metrics.Max)
                        metrics.Max = _heightMap[i, j];
                    if (_heightMap[i, j] < metrics.Min)
                        metrics.Min = _heightMap[i, j];
                }
            }
            if (metrics.Max - metrics.Min < (1020))
                _poiPossibleLocations.Add(new Vector2i() {x = x, y = z} );

            return metrics;
        }

        public void Save(string filePath)
        {
            _map.Save(Path.Combine(filePath, "splat3.png"));
        }
    }
}
