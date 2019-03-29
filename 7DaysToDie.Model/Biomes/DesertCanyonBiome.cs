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
    public class DesertCanyonBiome : BiomeBase
    {
        
        private float _canyonFactor;
        private float _riverFactor;
        private float riverLoweringFactor;
        private float _baseLevel;
        private float _cellNoiseMultiplier;

        private readonly INoise _featureRockNoise;
        private readonly INoise _generalLandscapeNoise;
        private readonly INoise _generalRollingBaseNoise;
        private readonly INoise _cellNoise;

        public DesertCanyonBiome(string baseDirectory, int size, NoiseFactory noiseFactory) 
            : base(Path.Combine(baseDirectory, nameof(DesertCanyonBiome)), 
                Biome.DesertColor, size, noiseFactory)
        {
            _featureRockNoise = noiseFactory.GetFeatureRockNoise((float)0.06, (float)0.1);
            _generalRollingBaseNoise = noiseFactory.GetRollingBaseLandscape((float)0.003);
            _generalLandscapeNoise = noiseFactory.GetPerlinFractalBillow((float)0.002);
            _cellNoise = noiseFactory.GetRollingBaseLandscape((float)0.05); //noiseFactory.GetCellularNoiseForLandscapeAddition((float)0.015);
        }

        public override void Generate()
        {
            using (var heightMap = new HeightMap(Size))
            {
                SetLevels();
                heightMap.Create();
                RegenerateHeightMap(heightMap);

                heightMap.Save(Path.Combine(BaseDirectory, "dtm.raw"));                
            }            
        }

        private void SetLevels()
        {
            _baseLevel = ((float)ushort.MaxValue / 10);
            _generalRollingBaseNoise.Amplitude = ((float)ushort.MaxValue / 6);
            _generalLandscapeNoise.Amplitude = ((float)ushort.MaxValue / 3);

            _cellNoise.Amplitude = (float)8;
            _cellNoiseMultiplier = (float)ushort.MaxValue / 30;

            _featureRockNoise.Amplitude = (float)ushort.MaxValue / 15;
            _riverFactor = _generalLandscapeNoise.Amplitude / (float)15;
            _canyonFactor = _generalLandscapeNoise.Amplitude - _generalLandscapeNoise.Amplitude / (float)1.5;
            riverLoweringFactor = (float)0.01;
        }

        public void RegenerateHeightMap(HeightMap heightMap)
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
        
        private float GetPixelShade(int x, int y)
        {
            float baseLandscape = _generalRollingBaseNoise.GetNoise(x, y);
            float level = _generalLandscapeNoise.GetNoise(x, y);

            if (level < _riverFactor)
            {
                level = _baseLevel + baseLandscape + level - (level * riverLoweringFactor);
            }
            else if (level > _canyonFactor)
            {
                level = _baseLevel + baseLandscape + level;
                var step = (float)(Math.Round((double)level / 3200) * 3200);
                if (step < level) step = level;
                level = step + _featureRockNoise.GetNoise(x, y);
            }
            else
            {
                level = _baseLevel + baseLandscape + level;
            }

            return level < 0 ? 0 : level;
        }

        
        
    }
}
