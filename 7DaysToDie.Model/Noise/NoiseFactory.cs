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

        public INoise GetCellularNoiseForLandscapeAddition(float freq = (float)0.0015)
        {
            var myNoise = new FastNoise(_random.Next(int.MinValue, int.MaxValue));
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);            
            myNoise.SetCellularJitter((float)0.45);
            myNoise.SetFrequency(freq);
            myNoise.SetGradientPerturbAmp((float)30.0);
            return new GeneralNoise(myNoise);
        }

        public INoise GetCellularNoiseForLandscape(float freq = (float)0.0015)
        {
            var myNoise = new FastNoise(_random.Next(int.MinValue, int.MaxValue));
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Sub);
            myNoise.SetCellularJitter((float)0.45);
            myNoise.SetFrequency(freq);
            myNoise.SetGradientPerturbAmp((float)30.0);

            return new GeneralNoise(myNoise);
        }

        public INoise GetCellularNoiseForMaze(float freq = (float)0.03)
        {
            var myNoise = new FastNoise(_random.Next(int.MinValue, int.MaxValue));
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Manhattan);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Div);
            myNoise.SetCellularJitter((float)0.45);
            myNoise.SetFrequency(freq);
            
            return new GeneralNoise(myNoise);
        }

        public INoise GetFeatureRockNoise(float featurefrequency, float featureSizeFrequency)
        {
            return new FeatureNoise(
                GetCellularNoiseForLandscapeAddition(featurefrequency), 
                GetCellularNoiseForRockFeatures(featureSizeFrequency));
        }

        public INoise GetCellularNoiseForRockFeatures(float freq = (float)0.04)
        {
            var myNoise = new FastNoise(_random.Next(int.MinValue, int.MaxValue));
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Sub);
            myNoise.SetCellularJitter((float)0.45);
            myNoise.SetFrequency(freq);
            return new GeneralNoise(myNoise);
        }


        public INoise GetValueFractalForRivers()
        {
            var myNoise = new FastNoise(_random.Next(int.MinValue, int.MaxValue));
            myNoise.SetNoiseType(FastNoise.NoiseType.ValueFractal);
            myNoise.SetFractalType(FastNoise.FractalType.RigidMulti);
            myNoise.SetInterp(FastNoise.Interp.Hermite);
            myNoise.SetFractalGain((float)0.9);
            myNoise.SetFractalOctaves(1);
            myNoise.SetFractalLacunarity((float)1.0);
            myNoise.SetFrequency((float).001);
            return new GeneralNoise(myNoise);
        }

        public INoise GetRollingBaseLandscape(float frequency = (float)0.005)
        {
            var myNoise = new FastNoise(_random.Next(int.MinValue, int.MaxValue));
            myNoise.SetNoiseType(FastNoise.NoiseType.Perlin);
            myNoise.SetFractalType(FastNoise.FractalType.Billow);
            myNoise.SetInterp(FastNoise.Interp.Hermite);
            myNoise.SetFrequency(frequency);
            return new GeneralNoise(myNoise);
        }

        public INoise GetPerlinFractalBillow(float frequency = (float)0.005)
        {
            var myNoise = new FastNoise(_random.Next(int.MinValue, int.MaxValue));
            myNoise.SetNoiseType(FastNoise.NoiseType.PerlinFractal);
            myNoise.SetFractalType(FastNoise.FractalType.Billow);
            myNoise.SetInterp(FastNoise.Interp.Hermite);
            myNoise.SetFractalGain((float)0.9);
            myNoise.SetFractalOctaves(1);
            myNoise.SetFractalLacunarity((float)1.0);
            myNoise.SetFrequency(frequency);
            return new GeneralNoise(myNoise);
        }

        public INoise GetPerlinHermite(float frequency = (float)0.005)
        {
            var myNoise = new FastNoise(_random.Next(int.MinValue, int.MaxValue));
            myNoise.SetNoiseType(FastNoise.NoiseType.Perlin);
            myNoise.SetFractalType(FastNoise.FractalType.FBM);
            myNoise.SetInterp(FastNoise.Interp.Hermite);
            myNoise.SetFractalGain((float)0.9);
            myNoise.SetFractalOctaves(1);
            myNoise.SetFractalLacunarity((float)1.0);
            myNoise.SetFrequency(frequency);
            return new GeneralNoise(myNoise);
        }


        public INoise GetCellularNoiseForRoads()
        {
            var myNoise = new FastNoise(_random.Next(int.MinValue, int.MaxValue));
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Euclidean);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Div);
            myNoise.SetFrequency((float).0015);
            return new GeneralNoise(myNoise);
        }
        
        public INoise GetCellularNoiseForBiome(float frequency)
        {            
            var myNoise = new FastNoise(_random.Next());
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);            
            myNoise.SetFrequency(frequency);
            return new GeneralNoise(myNoise);
        }
        
    }
}
