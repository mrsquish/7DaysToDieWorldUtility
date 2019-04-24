using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using _7DaysToDie.Base;
using _7DaysToDie.Model;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Roads
{
    public class RoadCellMapLong
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();
        private int _size;
        private Func<RoadCellLong, IEnumerable<RoadCellLong>> _getNeighbours;
        private Func<RoadCellLong, RoadCellLong, ulong> _getPathCost;
        private RoadCellLong[,] _map;
        private HeightMap<ushort> _heightMap;
        private int _cellSize;
        private ulong distanceFactor = 4;
        private ulong _elevationFactor = 256;

        private RoadCellLong _origin;
        private RoadCellLong _destination;
        private ulong _maximumCost;

        public RoadCellMapLong(HeightMap<ushort> heightMap, int resolutionScale)
        {
            _heightMap = heightMap;
            _cellSize = resolutionScale;
            _size = (_heightMap.Size / resolutionScale);
            _map = new RoadCellLong[_size, _size];            
            GenerateCellMetricsMap();
        }

        public async Task<RoadCellLong> BuildPath(RoadCellLong from, RoadCellLong to)
        {
            _origin = from;
            _destination = to;            
            _getPathCost = (cell, roadCell) =>  WalkDirectEstimate(cell, roadCell, GetUnitRouteCost);
            _destination.CostFromOrigin = long.MaxValue;
            _maximumCost = _getPathCost(_origin, _destination);
            _getNeighbours = Get4RadiusNeighbours;            
            await PathFindingUsingLinkedCells(from);
            return to;
        }

        public RoadCellLong this[int x, int z] => _map[x, z];

        public async Task PathFindingUsingLinkedCells(
            RoadCellLong waypoint
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
            RoadCellLong pointA,
            RoadCellLong pointB)
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
                    //_logger.Warn($"Found a faster route to destination [{pointA.X},{pointA.Z}] Cost: {_maximumCost}");
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
        

        private ulong GetDirectLineDistance(Vector2<int> arg, Vector2<int> destination)
        {            
            return GetDirectLineDistance(Math.Abs(arg.X - destination.X), Math.Abs(arg.Z - destination.Z));
        }

        private ulong GetDirectLineDistance(int a, int b)
        {
            return (ulong)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        private ulong WalkDirectEstimate(RoadCellLong arg, RoadCellLong destination, Func<RoadCellLong, RoadCellLong, ulong> getRouteCost)
        {
            try
            {
                var walker = new Vector2<int>(arg.X, arg.Z);
                var previousCell = arg;
                var factors = GetWalkerFactors(arg, destination);
                ulong totalWalkingCost = 0;
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
            return long.MaxValue;
        }

        private Vector2<float> GetWalkerFactors(RoadCellLong origin, RoadCellLong destination)
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

        private ulong GetUnitRouteCost(RoadCellLong pointA, RoadCellLong pointB)
        {
            var heightDifference = Math.Abs((long)pointA.Avg - (long)pointB.Avg);
            if (heightDifference > 512)
                return (ulong)heightDifference << 4;
            return (ulong)heightDifference;
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

        public RoadCellLong GetCellMetrics(int x, int z)
        {
            var mapX = x * _cellSize;
            var mapZ = z * _cellSize;
            var cellBoundary = _cellSize - 1;
            var metrics = new RoadCellLong()
            {
                
                Avg = (ushort)((
                           _heightMap[mapX + cellBoundary, mapZ] +
                           _heightMap[mapX + cellBoundary, mapZ + cellBoundary] +
                           _heightMap[mapX, mapZ + cellBoundary] +
                           _heightMap[mapX, mapZ]) / 4f),
                X = x,
                Z = z
            };            
            return metrics;
        }

        private IEnumerable<RoadCellLong> Get8Neighbours(RoadCellLong fromCell)
        {
            var neighbours = new List<RoadCellLong>();
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

        private IEnumerable<RoadCellLong> Get4RadiusNeighbours(RoadCellLong origin)
        {
            return GetRadiusNeighbours(origin, PathFinding.TwelveRadius4Points, 5.7);
        }
        
        private IEnumerable<RoadCellLong> Get12RadiusNeighbours(RoadCellLong origin)
        {
            return GetRadiusNeighbours(origin, PathFinding.TwelveRadius16Points, 23);
        }

        private IEnumerable<RoadCellLong> GetRadiusNeighbours(RoadCellLong origin, List<Vector2<int>> offSets, double backTrackDistanceCheck)
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
