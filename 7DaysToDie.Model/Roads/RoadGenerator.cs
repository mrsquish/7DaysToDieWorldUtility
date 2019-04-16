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
        private readonly int _cellMapSize;
        private readonly int _maxPoiCount;
        private readonly _7DaysToDie.Model.HeightMap _heightMap;
        private readonly RgbBitMap _map;
        private readonly int _poiMinDistance = 70;

        private int cellSize = 8;

        private RoadCell[,] cellGraph;
        private List<RoadCell> _poiPossibleLocations = new List<RoadCell>();
        private List<RoadCell> _poiLocations = new List<RoadCell>();
        private Random _random;

        public RoadGenerator(int size, int maxPoiCount , _7DaysToDie.Model.HeightMap heightMap)
        {
            _size = size;
            _cellMapSize = _size / cellSize;
            _maxPoiCount = maxPoiCount;
            _heightMap = heightMap;
            _map = new RgbBitMap(_size);
            //_map.Initialise(0,0,0);
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
            for (int poiIndex = 0; poiIndex < _poiLocations.Count-1; poiIndex++)
            {
                if (_poiLocations.Count > 2 && poiIndex == _poiLocations.Count - 1)
                {
                    GenerateRoad(_poiLocations[poiIndex], _poiLocations[0]);
                }
                else
                {
                    GenerateRoad(_poiLocations[poiIndex], _poiLocations[poiIndex + 1]);
                }
                
            }
        }

        private void GenerateRoad(RoadCell pointA, RoadCell pointB)
        {
            var bestPath = PathFinding.FindPath(pointA, pointB, GetNeighbours, GetDistance, GetEstimate);
            RenderPathToMap(bestPath);

        }

        private void RenderPathToMap(Path<RoadCell> path)
        {            
            foreach (var pathCell in path)
            {
                var vector = pathCell.ToVector2(cellSize);
                for (int z = vector.Z; z < vector.Z + cellSize - 1; z++)
                {
                    for (int x = vector.X; x < vector.X + cellSize -1; x++)
                    {
                        _map.SetPixel(x, z, 255, 0, 0);
                    }
                }
            }
        }

        

        private double GetEstimate(RoadCell arg, RoadCell destination)
        {
            return 1; GetStandardDistance(arg, destination);
            //return GetDirectLineDistance(Math.Abs(arg.AvgCellHeight - destination.AvgCellHeight), (int)GetDirectLineDistance(Math.Abs(arg.X - destination.X), Math.Abs(arg.Z - destination.Z)));
        }

        private double GetDirectLineDistance(int a, int b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        private double GetStandardDistance(RoadCell pointA, RoadCell pointB)
        {
            return (Math.Abs(pointA.X - pointB.X) * 256) +
                   (Math.Abs(pointA.Z - pointB.Z) * 256);
            return Math.Abs(pointA.Avg - pointB.Avg) + (Math.Abs(pointA.X - pointB.X)*256) +
                   (Math.Abs(pointA.Z - pointB.Z)*256);
        }

        private double GetUnitDistance(RoadCell pointA, RoadCell pointB)
        {
            var heightDifference = Math.Abs(pointA.Avg - pointB.Avg);
            if (heightDifference > 40)
                return 16 * Math.Abs(pointA.Avg - pointB.Avg) + (Math.Abs(pointA.X - pointB.X) * 256) +
                       (Math.Abs(pointA.Z - pointB.Z) * 256); 
            return Math.Abs(pointA.Avg - pointB.Avg) + (Math.Abs(pointA.X - pointB.X) * 256) +
                   (Math.Abs(pointA.Z - pointB.Z) * 256); 
        }

        private double GetDistance(RoadCell pointA, RoadCell pointB)
        {
            var distance = GetUnitDistance(pointA, pointB);
            return distance;
        }

        private IEnumerable<RoadCell> GetNeighbours(RoadCell fromCell)
        {
            var neighbours = new List<RoadCell>();
            if (fromCell.X > 0)
            {
                neighbours.Add(cellGraph[fromCell.X - 1, fromCell.Z]);
                if (fromCell.Z > 0)
                    neighbours.Add(cellGraph[fromCell.X - 1, fromCell.Z - 1]);
                if (fromCell.Z < _cellMapSize-1)
                    neighbours.Add(cellGraph[fromCell.X - 1, fromCell.Z + 1]);
            }

            if (fromCell.X < _cellMapSize - 1)
            {
                neighbours.Add(cellGraph[fromCell.X + 1, fromCell.Z]);
                if (fromCell.Z > 0)
                    neighbours.Add(cellGraph[fromCell.X + 1, fromCell.Z - 1]);
                if (fromCell.Z < _cellMapSize - 1)
                    neighbours.Add(cellGraph[fromCell.X + 1, fromCell.Z + 1]);
            }

            if (fromCell.Z > 0)
                neighbours.Add(cellGraph[fromCell.X, fromCell.Z - 1]);
            if (fromCell.Z < _cellMapSize - 1)
                neighbours.Add(cellGraph[fromCell.X, fromCell.Z + 1]);

            return neighbours;
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

        private RoadCell GetRandomPoiLocation()
        {
            var location = _poiPossibleLocations[_random.Next(0, _poiPossibleLocations.Count - 1)];
            while (_poiLocations.Any(cell => Math.Abs(cell.X - location.X) < _poiMinDistance && Math.Abs(cell.Z - location.Z) < _poiMinDistance))
            {
                location = _poiPossibleLocations[_random.Next(0, _poiPossibleLocations.Count - 1)];
            }
            return location;
        }

        private void GenerateCellMetricsMap()
        {
            cellGraph = new RoadCell[_cellMapSize, _cellMapSize];
            CellLoop((x, z) => cellGraph[x, z] = GetCellMetrics(x, z));
        }

        private void CellLoop(Action<int, int> cellAction)
        {
            for (int z = 0; z < _cellMapSize; z++)
            {
                for (int x = 0; x < _cellMapSize; x++)
                {
                    cellAction(x, z);                    
                }
            }
        }

        public RoadCell GetCellMetrics(int x, int z)
        {
            var metrics = new RoadCell()
            {
                Avg = ((
                        _heightMap[x + cellSize, z] +
                        _heightMap[x + cellSize, z + cellSize] +
                        _heightMap[x, z + cellSize] +
                        _heightMap[x, z]) / 4),
                Max = 0,
                Min = ushort.MaxValue,
                X = x,
                Z = z
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
                _poiPossibleLocations.Add(metrics);

            metrics.AvgCellHeight =(int)(metrics.Avg / 256);

            return metrics;
        }

        public void Save(string filePath)
        {
            _map.Save(Path.Combine(filePath, "splat3.png"));
        }
    }
}
