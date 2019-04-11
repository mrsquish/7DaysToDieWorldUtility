using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.LayoutRenderers.Wrappers;
using _7DaysToDie.Model;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Maze
{
    [Flags]
    public enum ValidDirection
    {
        None = 0,
        North = 1,        
        East = 2,
        South = 4,
        West = 8,
        ReservedSpace = 16
    }

    public static class MazeExtensions
    {
        public static Vector2<int> NeighborInDirection(this Vector2<int> currentPoint, ValidDirection direction)
        {
            int ZdirectionValue = (int)(direction & (ValidDirection.North | ValidDirection.South));
            int XdirectionValue = (int)(direction & (ValidDirection.East | ValidDirection.West));
            ZdirectionValue = Math.Sign((ZdirectionValue - 2) * ZdirectionValue);
            XdirectionValue = Math.Sign((XdirectionValue - 3) * XdirectionValue);
            return new Vector2<int>(currentPoint.X - XdirectionValue, currentPoint.Z + ZdirectionValue);
        }

        public static ValidDirection Reverse(this ValidDirection direction)
        {
            var directionValue = ((int) direction);
            if (directionValue > 2)
                return (ValidDirection)(directionValue >> 2);
            return (ValidDirection)(directionValue << 2);
        }

        public static Vector2<int> PointToNorth(this Vector2<int> currentPoint)
        {
            return new Vector2<int>(currentPoint.X, currentPoint.Z-1);
        }

        public static Vector2<int> PointToEast(this Vector2<int> currentPoint)
        {
            return new Vector2<int>(currentPoint.X+1, currentPoint.Z);
        }

        public static Vector2<int> PointToSouth(this Vector2<int> currentPoint)
        {
            return new Vector2<int>(currentPoint.X, currentPoint.Z+1);
        }
        public static Vector2<int> PointToWest(this Vector2<int> currentPoint)
        {
            return new Vector2<int>(currentPoint.X-1, currentPoint.Z);
        }
    }

    public class Maze
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();
        public ValidDirection[,] BitMap;
        public readonly int Height;
        public readonly int Width;
        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        
        public Maze(int size)
        {
            Width = size;
            Height = size;
            BitMap = new ValidDirection[size, size];
        }

        public void GenerateRecursiveBackTracker()
        {
            InitialiseArray();
            GenerateRandomBlockedRegions(10);
            var start = GetRandomStartingPoint();
            _logger.Info($"Starting At  [{start.X},{start.Z}]");
            GenerateRecursiveBackTracker(start);
            OpenBlockedRegions();
        }

        private void GenerateRecursiveBackTracker(Vector2<int> currentPoint)
        {
            var validDirectionsToTravel = GetValidDirections(currentPoint);
            _logger.Info($"Analysing Cell [{currentPoint.X},{currentPoint.Z}]");
            while (validDirectionsToTravel != ValidDirection.None)
            {                
                var randomDirection = GetRandomDirection(validDirectionsToTravel);
                var neighbor = currentPoint.NeighborInDirection(randomDirection);
                RemoveWall(randomDirection, currentPoint);
                RemoveWall(randomDirection.Reverse(), neighbor);
                GenerateRecursiveBackTracker(neighbor);                
                validDirectionsToTravel = GetValidDirections(currentPoint);
            }
            
        }

        private void GenerateRandomBlockedRegions(int numberOfRegions)
        {
            for (int i = 0; i < numberOfRegions; i++)
            {
                _logger.Info($"Generating Open Space [{i}]");
                GenerateRandomBlockedRegion(_random.Next(Width/5,Width/3));
            }
        }

        private void OpenBlockedRegions()
        {
            for (int z = 0; z < Height - 1; z++)
            {
                _logger.Info($"OpenBlockedRegions Line [{z + 1}]");
                for (int x = 0; x < Width - 1; x++)
                {
                    if (BitMap[x, z] == ValidDirection.ReservedSpace)
                    {
                        if (x > 0)
                            BitMap[x - 1, z] &= ~ValidDirection.East;
                        if (z > 0)
                            BitMap[x, z -1] &= ~ValidDirection.South;
                        if (x < Width-1)
                            BitMap[x + 1, z] &= ~ValidDirection.West;
                        if (z < Height-1)
                            BitMap[x, z+1] &= ~ValidDirection.North;
                    }
                }
            }
        }

        private void GenerateRandomBlockedRegion(int regionSize)
        {
            var startingPoint = new Vector2<int>(_random.Next(0,Width-1), _random.Next(0, Height-1));
            for (int i = 0; i < regionSize; i++)
            {
                BitMap[startingPoint.X, startingPoint.Z] = ValidDirection.ReservedSpace;                
                var randomDirection = GetRandomDirection(GetValidDirections(startingPoint));
                if (randomDirection == ValidDirection.None) 
                    break;
                startingPoint = startingPoint.NeighborInDirection(randomDirection);
            }
        }

        public void RenderToHeightMap(string path, int bitPerCell)
        {
            var shadeHeight = ushort.MaxValue - (ushort.MaxValue / 4);
            using (var heightMap = new _7DaysToDie.Model.HeightMap(Height * bitPerCell))
            {
                heightMap.Create();
                for (int z = 0; z < Height - 1; z++)
                {
                    _logger.Info($"Rendering Line [{z+1}]");
                    for (int x = 0; x < Width - 1; x++)
                    {
                        
                        if ((BitMap[x, z] & ValidDirection.North) != 0)
                        {
                            SetLine(x * bitPerCell, bitPerCell,
                                counter => heightMap.SetPixel(counter, z * bitPerCell, shadeHeight));
                        }

                        if ((BitMap[x, z] & ValidDirection.South) != 0)
                        {
                            SetLine(x * bitPerCell, bitPerCell, counter =>
                                heightMap.SetPixel(counter, z * bitPerCell + bitPerCell - 1, shadeHeight));
                        }

                        if ((BitMap[x, z] & ValidDirection.East) != 0)
                        {
                            SetLine(z * bitPerCell, bitPerCell, counter =>
                                heightMap.SetPixel(x * bitPerCell + bitPerCell - 1, counter, shadeHeight));
                        }

                        if ((BitMap[x, z] & ValidDirection.West) != 0)
                        {
                            SetLine(z * bitPerCell, bitPerCell, counter =>
                                heightMap.SetPixel(x * bitPerCell, counter, shadeHeight));
                        }
                    }
                    
                }

                _logger.Info($"Saving HeightMap");
                heightMap.Save(path);
            }
        }

        private void SetLine(int startX, int bitPerCell, Action<int> forLineAction)
        {
            for (int i = startX; i < startX + bitPerCell - 1; i++)
            {
                forLineAction(i);
            }
        } 

        private void RemoveWall(ValidDirection direction, Vector2<int> currentPoint)
        {
            BitMap[currentPoint.X, currentPoint.Z] &= ~direction;
        }

        private void InitialiseArray()
        {
            for (int z = 0; z < Height-1; z++)
            {
                for (int x = 0; x < Width-1; x++)
                {
                    BitMap[x,z] = (ValidDirection.North | ValidDirection.East | ValidDirection.South | ValidDirection.West);
                }
            }
        }

        private ValidDirection GetRandomDirection(ValidDirection validDirections)
        {
            if (HasSingleDirection(validDirections))
                return validDirections;
            
            var direction = ValidDirection.None;
            while (direction == ValidDirection.None)
            {
                var bitShift = _random.Next(0, 4);
                var directionValue = (1 << bitShift);
                var randomDirection = (ValidDirection)directionValue;
                direction = randomDirection & validDirections;
            }
            return direction;
        }

        private bool PointVisited(Vector2<int> currentPoint)
        {
            return BitMap[currentPoint.X, currentPoint.Z] != (ValidDirection.North | ValidDirection.East | ValidDirection.South | ValidDirection.West);
        }

        private bool HasSingleDirection(ValidDirection validDirections)
        {
            var validDirectionValue = ((int)validDirections);
            return validDirectionValue != 0 && 
                   (validDirectionValue & (validDirectionValue - 1)) == 0;
        }

        private ValidDirection GetValidDirections(Vector2<int> currentPoint)
        {
            var validDirections = ValidDirection.None;

            if (!IsOnNorthBoarder(currentPoint) && !PointVisited(currentPoint.PointToNorth()))
                validDirections = validDirections | ValidDirection.North;
            if (!IsOnEastBoarder(currentPoint) && !PointVisited(currentPoint.PointToEast()))
                validDirections = validDirections | ValidDirection.East;
            if (!IsOnSouthBoarder(currentPoint) && !PointVisited(currentPoint.PointToSouth()))
                validDirections = validDirections | ValidDirection.South;
            if (!IsOnWestBoarder(currentPoint) && !PointVisited(currentPoint.PointToWest()))
                validDirections = validDirections | ValidDirection.West;
            return validDirections;
        }

        private Vector2<int> GetRandomStartingPoint()
        {
            var point = new Vector2<int>();
            if (_random.Next(0,2) == 0)
            {
                // Random starting point on the North or South                
                point.X = _random.Next(0, 2) * (Width-1); //Either 0 or Width;
                point.Z = _random.Next(0,Height);
            }
            else
            {
                // Random starting point on the East or West
                point.X = _random.Next(0, Width); 
                point.Z = _random.Next(0, 2) * (Height-1);
            }
            return point;
        }

        public bool IsOnNorthBoarder(Vector2<int> currentPoint)
        {
            return currentPoint.Z == 0;
        }

        public bool IsOnEastBoarder(Vector2<int> currentPoint)
        {
            return currentPoint.X == Width-1;
        }

        public bool IsOnSouthBoarder(Vector2<int> currentPoint)
        {
            return currentPoint.Z == Height - 1;
        }

        public bool IsOnWestBoarder(Vector2<int> currentPoint)
        {
            return currentPoint.X == 0;
        }

    }
}
