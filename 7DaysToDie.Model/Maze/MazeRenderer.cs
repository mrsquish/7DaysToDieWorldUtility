using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using _7DaysToDie.Base;
using HeightMap = _7DaysToDie.Model.HeightMap;

namespace _7DaysToDie.Maze
{
    public class MazeRenderer : IDisposable
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly int _size;
        private readonly int _bitsPerCell;
        private readonly float _wallHeight;
        private readonly int _cellWallWidth;
        private readonly Func<int, int, ValidDirection> _getCellValue;
        private _7DaysToDie.Model.HeightMap _heightMap;

        public MazeRenderer(int size, int bitsPerCell, float wallHeight, int cellWallWidth,
            Func<int, int, ValidDirection> getCellValue, _7DaysToDie.Model.HeightMap heightMap)
        {
            _size = size;
            _bitsPerCell = bitsPerCell;
            _wallHeight = WorldSettings.GroundLevel + wallHeight * WorldSettings.UnitLevel;
            _cellWallWidth = cellWallWidth;
            _getCellValue = getCellValue;
            _heightMap = heightMap;
        }

        public MazeRenderer(int size, int bitsPerCell, float wallHeight, int cellWallWidth, Func<int,int,ValidDirection> getCellValue) 
        :this(size,bitsPerCell,wallHeight,cellWallWidth, getCellValue, new _7DaysToDie.Model.HeightMap(size * bitsPerCell))
        {
        }

        public void RenderToHeightMap()
        {
            _heightMap.Initialise(WorldSettings.GroundLevel);
            for (int z = 0; z < _size - 1; z++)
            {
                _logger.Info($"Rendering Line [{z + 1}]");
                for (int x = 0; x < _size - 1; x++)
                {                    
                    if ((_getCellValue(x, z) & ValidDirection.North) != 0)
                    {
                        RenderHorizontalCellWallNorth(x,z);
                    }

                    if ((_getCellValue(x, z) & ValidDirection.South) != 0)
                    {
                        RenderHorizontalCellWallSouth(x,z);
                    }

                    if ((_getCellValue(x, z) & ValidDirection.East) != 0)
                    {
                        RenderVerticalCellWallEast(x, z);
                    }

                    if ((_getCellValue(x, z) & ValidDirection.West) != 0)
                    {
                        RenderVerticalCellWallWest(x, z);
                    }
                }
            }
        }

        public void SaveHeightMap(string path)
        {
            _logger.Info($"Saving HeightMap");
            _heightMap.Save(path);
        }

        private void RenderHorizontalCellWallSouth(int cellX, int cellZ)
        {
            HorizontalLineOperation(cellX * _bitsPerCell, cellZ * _bitsPerCell + _bitsPerCell - _cellWallWidth, _bitsPerCell, RenderVerticalWallWidth);
        }

        private void RenderHorizontalCellWallNorth(int cellX, int cellZ)
        {
            HorizontalLineOperation(cellX * _bitsPerCell, cellZ * _bitsPerCell, _bitsPerCell, RenderVerticalWallWidth);
        }

        private void RenderVerticalCellWallEast(int cellX, int cellZ)
        {
            VerticalLineOperation(cellX * _bitsPerCell + _bitsPerCell - _cellWallWidth, cellZ * _bitsPerCell, _bitsPerCell, RenderHorizontalWallWidth);
        }

        private void RenderVerticalCellWallWest(int cellX, int cellZ)
        {
            VerticalLineOperation(cellX * _bitsPerCell, cellZ * _bitsPerCell, _bitsPerCell, RenderHorizontalWallWidth);
        }

        private void RenderVerticalWallWidth(int xStart, int zStart)
        {
            VerticalLineOperation(xStart, zStart, _cellWallWidth, (x, z) => _heightMap.SetPixel(x, z, _wallHeight));
        }
        
        private void RenderHorizontalWallWidth(int xStart, int zStart)
        {
            HorizontalLineOperation(xStart, zStart, _cellWallWidth, (x, z) => _heightMap.SetPixel(x, z, _wallHeight));
        }

        private void HorizontalLineOperation(int startX, int z, int length, Action<int, int> forLineAction)
        {
            LineOperation(z, startX, length, (passThroughZ, incrementX) => forLineAction(incrementX, passThroughZ));
        }

        private void VerticalLineOperation(int x, int startZ, int length, Action<int, int> forBitAction)
        {
            LineOperation(x, startZ, length, forBitAction);
        }

        private void LineOperation(int passThrough, int toIncrement, int length, Action<int, int> forBitAction)
        {
            for (int i = toIncrement; i < toIncrement + length; i++)
            {
                forBitAction(passThrough, i);
            }
        }

        public void Dispose()
        {
            _heightMap?.Dispose();
        }
    }
}
