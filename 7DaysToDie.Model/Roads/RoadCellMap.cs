using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using _7DaysToDie.Model;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Roads
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
        private int distanceFactor = 4;
        private float _elevationFactor = 256;

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

        public Path<RoadCell> BuildPathUsingAStar(RoadCell from, RoadCell to)
        {
            _getPathCost = (cell, roadCell) => WalkDirectEstimate(cell, roadCell, GetWeightedUnitDistance);
            return PathFinding.AStarPathFindingUsingPriorityQueue(from, to, Get4RadiusNeighbours, _getPathCost,
                _getPathCost);
        }

        public async Task<RoadCell> BuildPath(RoadCell from, RoadCell to)
        {
            _origin = from;
            _destination = to;            
            _getPathCost = (cell, roadCell) =>  WalkDirectEstimate(cell, roadCell, GetWeightedUnitDistance);
            _destination.CostFromOrigin = double.MaxValue;
            _maximumCost = _getPathCost(_origin, _destination);
            _getNeighbours = Get4RadiusNeighbours;            
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
                    _logger.Info("Too Costly - Bailing out");
                }
                else if (GetDirectLineDistance(waypoint, _destination) < distanceFactor)
                {                    
                    ConsiderRoute(waypoint, _destination);
                }
                else
                {
                    var neighbors = _getNeighbours(waypoint).ToList();
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor != null)
                        {
                            if (ConsiderRoute(waypoint, neighbor))
                            {
                                await PathFindingUsingLinkedCells(neighbor);
                            }
                        }
                        else
                        {
                            _logger.Info("Neighbor is null?");
                        }
                    }                    
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
                if (pointA.Previous == pointB)
                    return false; //We've backtracked completely
                if ((int) pointB.DistanceFromOrigin == 0)
                    pointB.DistanceFromOrigin = GetDirectLineDistance(_origin, pointB);
                if (pointB != _destination && pointB.DistanceFromOrigin <= pointA.DistanceFromOrigin)
                    return false; //We're circling back

                //_logger.Info($"At [{pointB.X},{pointB.Z}] {pointB.DistanceFromOrigin} from Origin");
                var costFromOrigin = pointA.CostFromOrigin + _getPathCost(pointA, pointB);
                if (costFromOrigin > _destination.CostFromOrigin)
                    return false;
                
                // PointB has already been hit on different route, only redirect when the 
                // cost is lower.
                if (pointB.Previous != null && pointB.CostFromOrigin <= costFromOrigin)
                    return false;

                // PointA already has a route assigned, only update it when the cost is lower
                if (pointA.Next != null && pointA.Next.CostFromOrigin <= costFromOrigin)
                    return false;

                pointB.Previous = pointA;
                pointA.Next = pointB;
                pointB.CostFromOrigin = costFromOrigin;

                if (pointB == _destination)
                {
                    _maximumCost = costFromOrigin;
                    _logger.Warn($"Found a faster route to destination [{pointA.X},{pointA.Z}] Cost: {_maximumCost}");
                    return false;
                }

                return (pointA != pointB);                   
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

        private double WalkDirectEstimate(RoadCell arg, RoadCell destination, Func<RoadCell, RoadCell, double> getRouteCost)
        {
            try
            {
                var walker = new Vector2<int>(arg.X, arg.Z);
                var previousCell = arg;
                var factors = GetWalkerFactors(arg, destination);
                double totalWalkingCost = 0;
                int stepCounter = 1;
                do
                {
                    walker.X = previousCell.X + (int) Math.Round(factors.X * stepCounter, 0);
                    walker.Z = previousCell.Z + (int) Math.Round(factors.Z * stepCounter, 0);
                    totalWalkingCost += getRouteCost(previousCell, _map[walker.X, walker.Z]);
                    stepCounter++;
                } while (walker.X != destination.X && walker.Z != destination.Z);                
                return totalWalkingCost;
            }
            catch (Exception exp)
            {
                _logger.Error(exp);                
            }
            return double.MaxValue;
        }

        private Vector2<float> GetWalkerFactors(RoadCell origin, RoadCell destination)
        {
            var xDistance = destination.X - origin.X;
            var zDistance = destination.Z - origin.Z;
            var factors = new Vector2<float>(Math.Sign(xDistance), Math.Sign(zDistance));
            if (Math.Abs(xDistance) > Math.Abs(zDistance))
                factors.Z = (float)zDistance / (float)Math.Abs(xDistance);
            if (Math.Abs(zDistance) > Math.Abs(xDistance))
                factors.X = (float)xDistance / (float)Math.Abs(zDistance);
            return factors;
        }

        private double GetWeightedUnitDistance(RoadCell pointA, RoadCell pointB)
        {
            //var directLineDistance = GetDirectLineDistance(pointA, pointB);
            var heightDifference = Math.Abs(pointA.Avg - pointB.Avg);
            //return heightDifference;
            /*
            var weightedDifference = Math.Pow(2, Math.Min(32, heightDifference / 124));
            return weightedDifference;// * (directLineDistance / distanceFactor);
            */
            if (heightDifference < _elevationFactor)
                return heightDifference;

            var heightFactor = 1 + Math.Min(8, Math.Floor(heightDifference / 256));
            
            return (heightFactor * heightDifference);
        }


        private double GetUnitDistance(RoadCell pointA, RoadCell pointB, int heightFactor)
        {
            var heightDifference = Math.Abs(pointA.Avg - pointB.Avg);
            
            return (heightFactor * heightDifference);
        }

        private double DiagonalDistance(Vector2<int> node, Vector2<int> goal)
        {
            var dx = Math.Abs(node.X - goal.X);
            var dy = Math.Abs(node.Z - goal.Z);
            return (dx + dy) + (1.414f - 2) * Math.Min(dx, dy);
        }

        private void GenerateCellMetricsMap()
        {
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
            var mapX = x * _cellSize;
            var mapZ = z * _cellSize;
            var cellBoundary = _cellSize - 1;
            var metrics = new RoadCell()
            {
                
                Avg = ((
                           _heightMap[mapX + cellBoundary, mapZ] +
                           _heightMap[mapX + cellBoundary, mapZ + cellBoundary] +
                           _heightMap[mapX, mapZ + cellBoundary] +
                           _heightMap[mapX, mapZ]) / 4),
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

        private IEnumerable<RoadCell> Get4RadiusNeighbours(RoadCell origin)
        {
            return GetRadiusNeighbours(origin, PathFinding.TwelveRadius4Points, 5.7);
        }
        
        private IEnumerable<RoadCell> Get12RadiusNeighbours(RoadCell origin)
        {
            return GetRadiusNeighbours(origin, PathFinding.TwelveRadius16Points, 23);
        }

        private IEnumerable<RoadCell> GetRadiusNeighbours(RoadCell origin, List<Vector2<int>> offSets, double backTrackDistanceCheck)
        {
            try
            {
                var neighbours = offSets
                    .Select(vector2 => new Vector2<int>(vector2.X + origin.X, vector2.Z + origin.Z));
                return neighbours
                    .Where(vector2 => vector2.X > 0 && vector2.X < _size
                                                    && vector2.Z > 0 && vector2.Z < _size)
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
