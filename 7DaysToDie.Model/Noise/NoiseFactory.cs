﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Noise
{
    public class NoiseFactory
    {
        private Random _random = new Random();

        public FastNoise GetCellularNoiseForLandscapeAddition(float freq = (float)0.0015)
        {
            var myNoise = new FastNoise();
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);            
            myNoise.SetCellularJitter((float)0.45);
            myNoise.SetFrequency(freq);
            myNoise.SetGradientPerturbAmp((float)30.0);

            return myNoise;
        }

        public FastNoise GetCellularNoiseForLandscape(float freq = (float)0.0015)
        {
            var myNoise = new FastNoise();
            myNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
            myNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
            myNoise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Sub);
            myNoise.SetCellularJitter((float)0.45);
            myNoise.SetFrequency(freq);
            myNoise.SetGradientPerturbAmp((float)30.0);
            
            return myNoise;
        }

        public FastNoise GetValueFractalForLandscape2()
        {
            var myNoise = new FastNoise();
            myNoise.SetNoiseType(FastNoise.NoiseType.ValueFractal);
            myNoise.SetFractalType(FastNoise.FractalType.Billow);
            myNoise.SetInterp(FastNoise.Interp.Hermite);
            myNoise.SetFractalGain((float)0.9);
            myNoise.SetFractalOctaves(1);
            myNoise.SetFractalLacunarity((float)1.0);
            myNoise.SetFrequency((float).005);
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
