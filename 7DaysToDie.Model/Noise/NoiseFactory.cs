using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Noise
{
    public class NoiseFactory
    {
        private Random _random = new Random();

        public FastNoise GetCellularNoiseForLandscape()
        {
            var myNoise = new FastNoise();
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Euclidean);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Sub);
            myNoise.SetFrequency((float).0015);
            return myNoise;
        }

        public FastNoise GetCellularNoiseForRoads()
        {
            var myNoise = new FastNoise();
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Euclidean);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Div);
            myNoise.SetFrequency((float).0015);
            return myNoise;
        }
        
        public FastNoise GetCellularNoiseForBiome(float frequency)
        {            
            var myNoise = new FastNoise(_random.Next());
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);            
            myNoise.SetFrequency(frequency);
            return myNoise;
        }
        
    }
}
