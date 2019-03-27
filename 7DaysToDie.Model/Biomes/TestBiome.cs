using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FreeImageAPI;
using _7DaysToDie.Model.Extensions;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDie.Model.Biomes
{
    public class TestBiome : BiomeBase
    {
        
        private float _canyonFactor;
        private float _pathLevel;                
        private float _cellNoiseMultiplier;
        private float _stepFactor;

        private readonly INoise _featureRockNoise;
        private readonly INoise _swampPathNoise;
        private readonly INoise _swampFillerNoise;
        private readonly INoise _generalRollingBaseNoise;
        private readonly INoise _cellNoise;

        public TestBiome(string baseDirectory, int size, NoiseFactory noiseFactory) 
            : base(Path.Combine(baseDirectory, nameof(TestBiome)), 
                Biome.DesertColor, size, noiseFactory)
        {
            
            _generalRollingBaseNoise = noiseFactory.GetRollingBaseLandscape((float)0.003);
            _swampPathNoise = noiseFactory.GetPerlinFractalBillow((float) 0.02);
            _swampFillerNoise = noiseFactory.GetPerlinHermite((float)0.04);
            _swampPathNoise.Invert = true;

            _featureRockNoise = noiseFactory.GetFeatureRockNoise((float)0.06, (float)0.1);
            _cellNoise = noiseFactory.GetRollingBaseLandscape((float)0.05); //noiseFactory.GetCellularNoiseForLandscapeAddition((float)0.015);
        }

        public override void Generate()
        {
            using (var heightMap = new GreyScalePNG(Size))
            {
                SetLevels();
                heightMap.Create();
                //heightMap.GenerateDummy16bitImage(Path.Combine(BaseDirectory, "dtm.png"));
                RegenerateHeightMap(heightMap);
                heightMap.SavePng(Path.Combine(BaseDirectory, "dtm.png"));
                heightMap.SaveRaw(Path.Combine(BaseDirectory, "dtm.raw"));
            }            
        }

        private void SetLevels()
        {
            _stepFactor = 1200;
            _swampPathNoise.Amplitude = ((float)ushort.MaxValue / 20);
            _swampFillerNoise.Amplitude = _stepFactor;
            _pathLevel = _swampPathNoise.Amplitude - _stepFactor;

            _generalRollingBaseNoise.Amplitude = ((float)ushort.MaxValue / 6);

            _cellNoise.Amplitude = (float) 8;// (float)ushort.MaxValue / 20;
            _cellNoiseMultiplier = (float) ushort.MaxValue / 30;

            _featureRockNoise.Amplitude = (float)ushort.MaxValue / 15;
            
            _canyonFactor = _swampPathNoise.Amplitude - _swampPathNoise.Amplitude / (float)1.5;
            
        }

        public void RegenerateHeightMap(GreyScalePNG heightMap)
        {            
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    heightMap.SetPixel(x, y, GetPixelShade(x, y));                    
                }
                _logger.Info($"Writing Line {y}");                
            }
        }
        
        private ushort GetPixelShade(int x, int y)
        {
            //float baseLandscape = _generalRollingBaseNoise.GetNoise(x, y);
            float level = _swampPathNoise.GetNoise(x, y);

            level = SeaLevel + (float)(Math.Round((double)level / _stepFactor) * _stepFactor);
            
            if (level < _pathLevel)
            {
                level = _pathLevel - _swampFillerNoise.Amplitude + _swampFillerNoise.GetNoise(x, y);
            }
            /*
            else if (level > _canyonFactor)
            {
                level = _baseLevel + baseLandscape + level;
                var step = (float) (Math.Round((double) level / 3200) * 3200);
                if (step < level) step = level;
                //level = _baseLevel + step;
                //level = (_cellNoise.GetNoise(x, y) * _cellNoiseMultiplier);              (_cellNoise.GetNoise(x, y) * _cellNoiseMultiplier);//
                level = step + _featureRockNoise.GetNoise(x, y);             
            }
            else
            {
                level = _baseLevel + baseLandscape + level;
            }
            */
            return level < 0 ? (ushort) 0 : (ushort) level;
        }

        
        
    }
}
