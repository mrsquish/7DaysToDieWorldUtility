using System;
using System.IO;
using _7DaysToDie.Model.Extensions;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDie.Model.Biomes
{
    public class NastyBiome : BiomeBase
    {

        private float _canyonFactor;
        private float _riverFactor;
        private float riverLoweringFactor;
        private float _baseLevel;

        private INoise _generalLandscapeNoise;
        private INoise _featureNoise;
        private INoise _cellNoise;

        public NastyBiome(string baseDirectory, int size, NoiseFactory noiseFactory)
            : base(
                Path.Combine(baseDirectory, nameof(NastyBiome)), 
                Biome.BurntForestColor, size, noiseFactory)
        {
            _featureNoise = noiseFactory.GetFeatureRockNoise((float)0.06, (float)0.1);
            _generalLandscapeNoise = noiseFactory.GetCellularNoiseForLandscapeAddition((float)0.015); 
            _cellNoise = noiseFactory.GetCellularNoiseForMaze();
        }

        public override void Generate()
        {
            using (var heightMap = new GreyScalePNG(Size))
            {
                SetLevels();
                heightMap.Create();                
                RegenerateHeightMap(heightMap);
                heightMap.Save(Path.Combine(BaseDirectory, "dtm.png"));
            }
        }

        private void SetLevels()
        {
            _baseLevel = ((float)ushort.MaxValue / 4);
            _generalLandscapeNoise.Amplitude = ((float)ushort.MaxValue / 5);
            _cellNoise.Amplitude = (float)ushort.MaxValue;
            _featureNoise.Amplitude = (float)ushort.MaxValue / 15;
            _riverFactor = _baseLevel + _cellNoise.Amplitude / (float)15;
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
            float level = _baseLevel + _cellNoise.GetNoise(x, y);

            if (level < _riverFactor)
            {
                level = _baseLevel;
            }
            else
            {
                level = _baseLevel + _generalLandscapeNoise.GetNoise(x, y);
                //+ _featureNoise.GetNoise(x, y);
            }

            return level < 0 ? (ushort)0 : (ushort)level;
        }

    }
}