using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Noise
{
    public class NoiseFactory
    {
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

        public FastNoise GetCellularNoiseForBiome()
        {
            var myNoise = new FastNoise();
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);
            myNoise.SetFrequency((float).0010);
            return myNoise;
        }
    }
}
