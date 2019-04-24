using _7DaysToDie.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NLog;
using _7DaysToDie.Base;
using _7DaysToDie.Contracts;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Roads
{
    

    public class RoadGenerator<T>
        where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();        
        private readonly int _maxPoiCount;
        private readonly Func<RoadCellMap<T>> _getCellMap;
        private RgbBitMap _map;
        private readonly int _poiMinDistance = 70;

        private int cellSize = 8;
        
        private List<ITypedPathCell<T>> _poiPossibleLocations = new List<ITypedPathCell<T>>();
        private List<ITypedPathCell<T>> _poiLocations = new List<ITypedPathCell<T>>();
        private Random _random;

        public RoadGenerator(int maxPoiCount, Func<RoadCellMap<T>> getCellMap)
        {                        
            _maxPoiCount = maxPoiCount;
            _getCellMap = getCellMap;            
            _random = new Random(DateTime.Now.Millisecond + (DateTime.Now.Minute <<  9));
        }

        public void Generate()
        {            
            //FindPoiLocations();
            GenerateRoadsBetweenPois();
        }

        private void GenerateRoadsBetweenPois()
        {
            var roadMap = _getCellMap();
            _map = new RgbBitMap(roadMap.HeightMap.Size);
            _poiLocations.Add(roadMap[100, 15]);
            _poiLocations.Add(roadMap[40, 120]);

            for (int poiIndex = 0; poiIndex < _poiLocations.Count-1; poiIndex++)
            {
                if (_poiLocations.Count > 2 && poiIndex == _poiLocations.Count - 1)
                {
                    GenerateRoad(_poiLocations[poiIndex], _poiLocations[0],roadMap);
                }
                else
                {
                    GenerateRoad(_poiLocations[poiIndex], _poiLocations[poiIndex + 1], roadMap);
                }
                
            }
        }

        private void GenerateRoad(ITypedPathCell<T> pointA, ITypedPathCell<T> pointB, RoadCellMap<T> roadMap)
        {
            var end = roadMap.BuildPath(pointA, pointB).Result;                        
            RenderPathToMap(pointB);            
            RenderVectorSquare(pointA, 0, 255, 0);
            RenderVectorSquare(pointB, 0, 255, 0);            
        }

        private void RenderPathToMap(Path<ITypedPathCell<T>> path)
        {
            foreach (var pathCell in path)
            {
                var vector = pathCell.ToVector2(cellSize);
                RenderVectorSquare(pathCell, 255, 0, 0);
            }
        }
        
        private void RenderPathToMap(ITypedPathCell<T> pathCell)
        {
            while (pathCell != null)
            {                
                RenderVectorSquare(pathCell, 255, 0, 0);
                pathCell = pathCell.Previous;
            }
        }

        private void RenderVectorSquare(ITypedPathCell<T> vector, byte Red, byte Green, byte Blue)
        {
            RenderVectorSquare(new Vector2<int>{ X= vector.X* cellSize, Z = vector.Z * cellSize}, Red, Green, Blue);
        }

        private void RenderVectorSquare(Vector2<int> vector, byte Red, byte Green, byte Blue)
        {
            for (int z = vector.Z; z < vector.Z + cellSize - 1; z++)
            {
                for (int x = vector.X; x < vector.X + cellSize - 1; x++)
                {
                    _map.SetPixel(x, z, Red, Green, Blue);
                }
            }
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

        private ITypedPathCell<T> GetRandomPoiLocation()
        {
            var location = _poiPossibleLocations[_random.Next(0, _poiPossibleLocations.Count - 1)];
            while (_poiLocations.Any(cell => Math.Abs(cell.X - location.X) < _poiMinDistance && Math.Abs(cell.Z - location.Z) < _poiMinDistance))
            {
                location = _poiPossibleLocations[_random.Next(0, _poiPossibleLocations.Count - 1)];
            }
            return location;
        }



        public void Save(string filePath)
        {
            _map.Save(Path.Combine(filePath, "splat3.png"));
        }
    }
}
