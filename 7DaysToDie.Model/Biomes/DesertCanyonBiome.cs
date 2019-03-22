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

        private readonly INoise _featureRockNoise;
        private readonly INoise _generalLandscapeNoise;
        private readonly INoise _cellNoise;

        public DesertCanyonBiome(string baseDirectory, int size, NoiseFactory noiseFactory) 
            : base(Path.Combine(baseDirectory, nameof(DesertCanyonBiome)), 
                Biome.DesertColor, size, noiseFactory)
        {
            _featureRockNoise = noiseFactory.GetFeatureRockNoise((float)0.06, (float)0.1);
            _generalLandscapeNoise = noiseFactory.GetValueFractalForDesertLandscape();
            _cellNoise = noiseFactory.GetCellularNoiseForLandscapeAddition((float)0.015);
        }

        public override void Generate()
        {
            using (var heightMap = new GreyScalePNG(Size))
            {
                SetLevels();
                heightMap.Create();
                //heightMap.GenerateDummy16bitImage(Path.Combine(BaseDirectory, "dtm.png"));
                RegenerateHeightMap(heightMap);
                heightMap.Save(Path.Combine(BaseDirectory, "dtm.png"));
            }            
        }

        private void SetLevels()
        {
            _baseLevel = ((float)ushort.MaxValue / 8);
            _generalLandscapeNoise.Amplitude = ((float)ushort.MaxValue/3);
            _cellNoise.Amplitude = (float)ushort.MaxValue / 20;
            _featureRockNoise.Amplitude = (float)ushort.MaxValue / 15;
            _riverFactor = _baseLevel + _generalLandscapeNoise.Amplitude / (float)15;
            _canyonFactor = _baseLevel + _generalLandscapeNoise.Amplitude 
                            - _generalLandscapeNoise.Amplitude / (float)1.5;
            riverLoweringFactor = (float)0.1;            
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
            float level = _baseLevel + _generalLandscapeNoise.GetNoise(x, y);
            
            if (level < _riverFactor)
            {
                level = level - (level * riverLoweringFactor); 
            }
            else if (level > _canyonFactor)
            {
                level = level  + _cellNoise.GetNoise(x, y)
                            + _featureRockNoise.GetNoise(x, y);             
            }
            else
            {
                //Debug.Print("Normal");
            }            
            return level < 0 ? (ushort) 0 : (ushort) level;
        }

        
        
    }
}
