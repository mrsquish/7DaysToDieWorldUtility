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
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Roads
{
    

    public class RoadGenerator
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();        
        private readonly int _maxPoiCount;
        private readonly _7DaysToDie.Model.HeightMap _heightMap;
        private readonly RgbBitMap _map;
        private readonly int _poiMinDistance = 70;

        private int cellSize = 8;
        
        private List<RoadCell> _poiPossibleLocations = new List<RoadCell>();
        private List<RoadCell> _poiLocations = new List<RoadCell>();
        private Random _random;

        public RoadGenerator(int maxPoiCount , _7DaysToDie.Model.HeightMap heightMap)
        {                        
            _maxPoiCount = maxPoiCount;
            _heightMap = heightMap;
            _map = new RgbBitMap(heightMap.Size);
            //_map.Initialise(0,0,0);
            _random = new Random(DateTime.Now.Millisecond + (DateTime.Now.Minute <<  9));
        }

        public void Generate()
        {
            
            //FindPoiLocations();
            GenerateRoadsBetweenPois();
        }

        private void GenerateRoadsBetweenPois()
        {
            var roadMap = new RoadCellMap(_heightMap, 8);
            _poiLocations.Add(roadMap[110, 10]);
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

        private void GenerateRoad(RoadCell pointA, RoadCell pointB, RoadCellMap roadMap)
        {
            var end = roadMap.BuildPath(pointA, pointB).Result;
            RenderPathToMap(pointA);
            RenderVectorSquare(pointA, 0, 255, 0);
            RenderVectorSquare(pointB, 0, 255, 0);            
        }

        private void RenderPathToMap(RoadCell pathCell)
        {
            while (pathCell.Next != null)
            {                
                RenderVectorSquare(pathCell, 255, 0, 0);
                pathCell = pathCell.Next;
            }
        }

        private void RenderVectorSquare(RoadCell vector, byte Red, byte Green, byte Blue)
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

        private RoadCell GetRandomPoiLocation()
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
