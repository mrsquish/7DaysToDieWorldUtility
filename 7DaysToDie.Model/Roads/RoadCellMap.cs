using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NLog;
using _7DaysToDie.Model.Model;
using _7DaysToDie.Roads;

namespace _7DaysToDie.Model
{
    public class RoadCellMap
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();
        private int _size;
        private Func<RoadCell, IEnumerable<RoadCell>> _getNeighbours;
        private Func<RoadCell, RoadCell, double> _getPathCost;
        private RoadCell[,] _map;
        private _7DaysToDie.Model.HeightMap _heightMap;
        private int _cellSize;

        private RoadCell _origin;
        private RoadCell _destination;
        private double _maximumCost;

        public RoadCellMap(_7DaysToDie.Model.HeightMap heightMap, int resolutionScale)
        {
            _heightMap = heightMap;
            _cellSize = resolutionScale;
            _size = (_heightMap.Size / resolutionScale);
            _map = new RoadCell[_size, _size];
            GenerateCellMetricsMap();
        }

        public async Task<RoadCell> BuildPath(RoadCell from, RoadCell to)
        {
            _origin = from;
            _destination = to;            
            _getPathCost = (cell, roadCell) =>  WalkDirectEstimate(cell, roadCell, GetUnitDistance);
            _destination.CostFromOrigin = double.MaxValue;
            _maximumCost = _getPathCost(_origin, _destination) * 2;
            _getNeighbours = Get12RadiusNeighbours;            
            await PathFindingUsingLinkedCells(from);
            return to;
        }

        public RoadCell this[int x, int z] => _map[x, z];

        public async Task PathFindingUsingLinkedCells(
            RoadCell waypoint
        )
        {
            try
            {
                if (waypoint.CostFromOrigin > _maximumCost)
                {
                    _logger.Warn("Too Costly - Bailing out");
                }
                else if (GetDirectLineDistance(waypoint, _destination) < 16)
                {
                    _logger.Warn("Reached Destination");
                    ConsiderRoute(waypoint, _destination);
                }
                else
                {
                    var recursingNeighbors = _getNeighbours(waypoint).Select(async x =>
                    {
                        if (ConsiderRoute(waypoint, x))
                        {
                            await PathFindingUsingLinkedCells(x);
                        }
                    });
                    await Task.WhenAll(recursingNeighbors);
                }
            }
            catch (Exception exp)
            {
                _logger.Error(exp);
                throw;
            }
        }

        private bool ConsiderRoute(
            RoadCell pointA,
            RoadCell pointB)
        {
            try
            {
                lock (pointB)
                {
                    if (pointA.Previous == pointB)
                        return false; //We've backtracked completely
                    if ((int) pointB.DistanceFromOrigin == 0)
                        pointB.DistanceFromOrigin = GetDirectLineDistance(_origin, pointB);
                    if (pointB.DistanceFromOrigin <= pointA.DistanceFromOrigin)
                        return false; //We're circling back

                    _logger.Info($"At [{pointB.X},{pointB.Z}] {pointB.DistanceFromOrigin} from Origin");
                    var costFromOrigin = pointA.CostFromOrigin + _getPathCost(pointA, pointB);
                    if (costFromOrigin > _destination.CostFromOrigin)
                        return false;
                    
                    if (pointB.Previous != null && pointB.CostFromOrigin < costFromOrigin)
                        return false;

                    if (pointB.Next != null)
                        _logger.Warn($"Found a faster route! [{pointB.X},{pointB.Z}]");

                    _logger.Info($"Persuing [{pointB.X},{pointB.Z}]");
                    pointB.Previous = pointA;
                    pointA.Next = pointB;
                    pointB.CostFromOrigin = costFromOrigin;
                    return (pointA != pointB);                   
                }

            }
            catch (Exception exp)
            {
                _logger.Error(exp);
                throw;
            }
        }


        private double GetDirectLineDistance(Vector2<int> arg, Vector2<int> destination)
        {            
            return GetDirectLineDistance(Math.Abs(arg.X - destination.X), Math.Abs(arg.Z - destination.Z));
        }

        private double GetDirectLineDistance(int a, int b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        private double WalkDirectEstimate(RoadCell arg, RoadCell destination, Func<RoadCell, RoadCell, int, double> getDistance)
        {
            var walker = new Vector2<int>(arg.X, arg.Z);
            var previousCell = arg;
            var factors = GetWalkerFactors(arg, destination);
            double totalWalkingCost = 0;
            int stepCounter = 1;
            do
            {
                walker.X = (int)Math.Round(factors.X * stepCounter, 0);
                walker.Z = (int)Math.Round(factors.Z * stepCounter, 0);
                totalWalkingCost += getDistance(previousCell, _map[walker.X, walker.Z], 8);
                stepCounter++;
            } while (walker.X != destination.X && walker.Z != destination.Z);
            //_logger.Info($"From ({arg.X},{arg.Z})->({destination.X},{destination.Z}) Distance = {totalWalkingCost}");
            return totalWalkingCost;
        }

        private Vector2<float> GetWalkerFactors(RoadCell arg, RoadCell destination)
        {
            var xDistance = arg.X - destination.X;
            var zDistance = arg.Z - destination.Z;
            var factors = new Vector2<float>(1, 1);
            if (Math.Abs(xDistance) > Math.Abs(zDistance))
                factors.Z = (float)Math.Abs(zDistance) / (float)Math.Abs(xDistance);
            if (Math.Abs(zDistance) > Math.Abs(xDistance))
                factors.X = (float)Math.Abs(xDistance) / (float)Math.Abs(zDistance);
            return factors;
        }

        private double GetUnitDistance(RoadCell pointA, RoadCell pointB, int heightFactor)
        {
            var heightDifference = Math.Abs(pointA.Avg - pointB.Avg);
            var diagonalDistance = 0;//DiagonalDistance(pointA, pointB);
            return (heightFactor * heightDifference) + (diagonalDistance * 256);
        }

        private double DiagonalDistance(Vector2<int> node, Vector2<int> goal)
        {
            var dx = Math.Abs(node.X - goal.X);
            var dy = Math.Abs(node.Z - goal.Z);
            return (dx + dy) + (1.414f - 2) * Math.Min(dx, dy);
        }

        private void GenerateCellMetricsMap()
        {
            _map = new RoadCell[_size, _size];
            CellLoop((x, z) => _map[x, z] = GetCellMetrics(x, z));
        }

        private void CellLoop(Action<int, int> cellAction)
        {
            for (int z = 0; z < _size; z++)
            {
                for (int x = 0; x < _size; x++)
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
                           _heightMap[x + _cellSize, z] +
                           _heightMap[x + _cellSize, z + _cellSize] +
                           _heightMap[x, z + _cellSize] +
                           _heightMap[x, z]) / 4),
                X = x,
                Z = z
            };
            /*
            for (int i = z; i <= (z - _cellSize); i++)
            {
                for (int j = x; j < (x - _cellSize); j++)
                {
                    if (_heightMap[i, j] > metrics.Max)
                        metrics.Max = _heightMap[i, j];
                    if (_heightMap[i, j] < metrics.Min)
                        metrics.Min = _heightMap[i, j];
                }
            }*/
            return metrics;
        }

        private IEnumerable<RoadCell> Get8Neighbours(RoadCell fromCell)
        {
            var neighbours = new List<RoadCell>();
            if (fromCell.X > 0)
            {
                neighbours.Add(_map[fromCell.X - 1, fromCell.Z]);
                if (fromCell.Z > 0)
                    neighbours.Add(_map[fromCell.X - 1, fromCell.Z - 1]);
                if (fromCell.Z < _size - 1)
                    neighbours.Add(_map[fromCell.X - 1, fromCell.Z + 1]);
            }

            if (fromCell.X < _size - 1)
            {
                neighbours.Add(_map[fromCell.X + 1, fromCell.Z]);
                if (fromCell.Z > 0)
                    neighbours.Add(_map[fromCell.X + 1, fromCell.Z - 1]);
                if (fromCell.Z < _size - 1)
                    neighbours.Add(_map[fromCell.X + 1, fromCell.Z + 1]);
            }

            if (fromCell.Z > 0)
                neighbours.Add(_map[fromCell.X, fromCell.Z - 1]);
            if (fromCell.Z < _size - 1)
                neighbours.Add(_map[fromCell.X, fromCell.Z + 1]);

            return neighbours;
        }

        private IEnumerable<RoadCell> Get12RadiusNeighbours(RoadCell origin)
        {
            try
            {
                var neighbours = PathFinding
                    .TwelveRadius16Points
                    .Select(vector2 => new Vector2<int>(vector2.X + origin.X, vector2.Z + origin.Z));
                return neighbours
                    .Where(vector2 => vector2.X > 0 && vector2.X < _size
                                      && vector2.Z > 0 && vector2.Z < _size
                                      && (origin.Previous == null
                                          || GetDirectLineDistance(origin.Previous, vector2) > 23))
                    .Select(vector2 => _map[vector2.X, vector2.Z]);

            }
            catch (Exception exp)
            {
                _logger.Error(exp);
                throw;
            }
        }
    }
}
