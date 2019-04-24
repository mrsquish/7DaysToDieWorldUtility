using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using _7DaysToDie.Base;
using _7DaysToDie.Contracts;
using _7DaysToDie.Model;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Roads
{
    public class RoadCellMap<T>
        where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();
        protected Func<ITypedPathCell<T>, IEnumerable<ITypedPathCell<T>>> _getNeighbours;
        protected Func<ITypedPathCell<T>, ITypedPathCell<T>, T> _getPathCost;

        private int _size;
        private ITypedPathCell<T>[,] _map;
        private HeightMap<ushort> _heightMap;
        private int _cellSize;
        private readonly Func<T> _getMaxValue;
        private readonly Func<T, T, T> _incrementFunc;
        private readonly Func<ITypedPathCell<T>, ITypedPathCell<T>, T> _getUnitRouteCost;
        private readonly Func<ITypedPathCell<T>> _getNew;
        private ulong distanceFactor = 4;
        private ulong _elevationFactor = 256;

        private ITypedPathCell<T> _origin;
        private ITypedPathCell<T> _destination;
        private T _maximumCost;

        public RoadCellMap(
            HeightMap<ushort> heightMap,
            int resolutionScale,
            Func<T> getMaxValue,
            Func<T,T,T> incrementFunc,
            Func<ITypedPathCell<T>, ITypedPathCell<T>,T> getUnitRouteCost,
            Func<ITypedPathCell<T>> getNew)
        {
            _heightMap = heightMap;
            _cellSize = resolutionScale;
            _getMaxValue = getMaxValue;
            _incrementFunc = incrementFunc;
            _getUnitRouteCost = getUnitRouteCost;
            _getNew = getNew;
            _size = (_heightMap.Size / resolutionScale);
            _map = new ITypedPathCell<T>[_size, _size];
            _getPathCost = (cell, roadCell) => WalkDirectEstimate(cell, roadCell, _getUnitRouteCost);
            _getNeighbours = Get4RadiusNeighbours;
            GenerateCellMetricsMap();
        }

        public HeightMap<ushort> HeightMap => _heightMap;

        public async Task<ITypedPathCell<T>> BuildPath(ITypedPathCell<T> from, ITypedPathCell<T> to)
        {
            _origin = from;
            _destination = to;
            _destination.CostFromOrigin = _getMaxValue();
            _maximumCost = _getPathCost(_origin, _destination);
            await PathFindingUsingLinkedCells(from);
            return to;
        }

        public ITypedPathCell<T> this[int x, int z] => _map[x, z];

        public async Task PathFindingUsingLinkedCells(
            ITypedPathCell<T> waypoint
        )
        {
            try
            {
                if (waypoint.ReachedMaximumCost(_maximumCost))
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
            ITypedPathCell<T> pointA,
            ITypedPathCell<T> pointB)
        {
            try
            {
                if (pointA.Previous == pointB)
                    return false; //We've backtracked completely
                if ((int)pointB.DistanceFromOrigin == 0)
                    pointB.DistanceFromOrigin = GetDirectLineDistance(_origin, pointB);
                if (pointB != _destination && pointB.DistanceFromOrigin <= pointA.DistanceFromOrigin)
                    return false; //We're circling back

                //_logger.Info($"At [{pointB.X},{pointB.Z}] {pointB.DistanceFromOrigin} from Origin");
                var costFromOrigin = pointA.CombineCost(_getPathCost(pointA, pointB));
                if (_destination.HasLowerCost(costFromOrigin))
                    return false;

                // PointB has already been hit on different route, only redirect when the 
                // cost is lower.
                if (pointB.Previous != null && pointB.HasLowerCost(costFromOrigin))
                    return false;

                // PointA already has a route assigned, only update it when the cost is lower
                if (pointA.Next != null && pointA.Next.HasLowerCost(costFromOrigin))
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

        private double GetDirectLineDistance(IVector2<int> arg, IVector2<int> destination)
        {
            return GetDirectLineDistance(Math.Abs(arg.X - destination.X), Math.Abs(arg.Z - destination.Z));
        }

        private double GetDirectLineDistance(int a, int b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        private T WalkDirectEstimate(
            ITypedPathCell<T> arg,
            ITypedPathCell<T> destination,
            Func<ITypedPathCell<T>, ITypedPathCell<T>, T> getRouteCost)
        {
            try
            {
                var walker = new Vector2<int>(arg.X, arg.Z);
                var previousCell = arg;
                var factors = GetWalkerFactors(arg, destination);
                T totalWalkingCost = new T();
                int stepCounter = 1;
                do
                {
                    walker.X = previousCell.X + (int)Math.Round(factors.X * stepCounter, 0);
                    walker.Z = previousCell.Z + (int)Math.Round(factors.Z * stepCounter, 0);
                    totalWalkingCost = _incrementFunc(totalWalkingCost, getRouteCost(previousCell, _map[walker.X, walker.Z]));
                    stepCounter++;
                } while (walker.X != destination.X && walker.Z != destination.Z);
                return totalWalkingCost;
            }
            catch (Exception exp)
            {
                _logger.Error(exp);
                throw;
            }
        }

        private Vector2<float> GetWalkerFactors(ITypedPathCell<T> origin, ITypedPathCell<T> destination)
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

        public ITypedPathCell<T> GetCellMetrics(int x, int z)
        {
            var metrics = _getNew();
            metrics.X = x;
            metrics.Z = z;

            metrics.SetAverage(GetCellAverage(x, z));
            return metrics;
        }


        protected double GetCellAverage(int x, int z)
        {
            var mapX = x * _cellSize;
            var mapZ = z * _cellSize;
            var cellBoundary = _cellSize - 1;
            return (
                           _heightMap[mapX + cellBoundary, mapZ] +
                           _heightMap[mapX + cellBoundary, mapZ + cellBoundary] +
                           _heightMap[mapX, mapZ + cellBoundary] +
                           _heightMap[mapX, mapZ]) / 4f;
        }



        private IEnumerable<ITypedPathCell<T>> Get8Neighbours(ITypedPathCell<T> fromCell)
        {
            var neighbours = new List<ITypedPathCell<T>>();
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

        private IEnumerable<ITypedPathCell<T>> Get4RadiusNeighbours(ITypedPathCell<T> origin)
        {
            return GetRadiusNeighbours(origin, PathFinding.TwelveRadius4Points, 5.7);
        }

        private IEnumerable<ITypedPathCell<T>> Get12RadiusNeighbours(ITypedPathCell<T> origin)
        {
            return GetRadiusNeighbours(origin, PathFinding.TwelveRadius16Points, 23);
        }

        private IEnumerable<ITypedPathCell<T>> GetRadiusNeighbours(ITypedPathCell<T> origin, List<Vector2<int>> offSets, double backTrackDistanceCheck)
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
