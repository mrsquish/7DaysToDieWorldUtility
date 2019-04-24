using System;
using System.IO;
using _7DaysToDie.Model.Extensions;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDie.Model.Biomes
{
    public class DesertCanyonBiome : BiomeBase
    {
        private readonly INoise _cellNoise;

        private readonly INoise _featureRockNoise;
        private readonly INoise _generalLandscapeNoise;
        private readonly INoise _generalRollingBaseNoise;

        private float _canyonFactor;
        private float _cellNoiseMultiplier;
        private float _riverFactor;
        private float riverLoweringFactor;

        public DesertCanyonBiome(string baseDirectory, int size, NoiseFactory noiseFactory)
            : base(Path.Combine(baseDirectory, nameof(DesertCanyonBiome)),
                Biome.DesertColor, size, noiseFactory)
        {
            _featureRockNoise = noiseFactory.GetFeatureRockNoise((float) 0.06, (float) 0.1);
            _generalRollingBaseNoise = noiseFactory.GetRollingBaseLandscape((float) 0.003);
            _generalLandscapeNoise = noiseFactory.GetPerlinFractalBillow((float) 0.002);
            _cellNoise =
                noiseFactory
                    .GetRollingBaseLandscape(
                        (float) 0.05); //noiseFactory.GetCellularNoiseForLandscapeAddition((float)0.015);
        }

        public override void Generate()
        {
            var heightMap = new HeightMap(Size);
            SetLevels();
            heightMap.Create();
            RegenerateHeightMap(heightMap);

            heightMap.Save(Path.Combine(BaseDirectory, "dtm.raw"));
            
        }

        private void SetLevels()
        {
            _generalRollingBaseNoise.Amplitude = (float) ushort.MaxValue / 6;
            _generalLandscapeNoise.Amplitude = (float) ushort.MaxValue / 3;

            _cellNoise.Amplitude = 8;
            _cellNoiseMultiplier = (float) ushort.MaxValue / 30;

            _featureRockNoise.Amplitude = (float) ushort.MaxValue / 15;
            _riverFactor = _generalLandscapeNoise.Amplitude / 15;
            _canyonFactor = _generalLandscapeNoise.Amplitude - _generalLandscapeNoise.Amplitude / (float) 1.5;
            riverLoweringFactor = (float) 0.01;
        }

        public void RegenerateHeightMap(HeightMap heightMap)
        {
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++) heightMap.SetPixel(x, y, GetPixelShade(x, y));
                _logger.Info($"Writing Line {y}");
            }
        }

        private float GetPixelShade(int x, int y)
        {
            var baseLandscape = _generalRollingBaseNoise.GetNoise(x, y);
            var level = _generalLandscapeNoise.GetNoise(x, y);

            if (level < _riverFactor)
            {
                level = BaseLevel + baseLandscape + level - level * riverLoweringFactor;
            }
            else if (level > _canyonFactor)
            {
                level = BaseLevel + baseLandscape + level;
                var step = (float) (Math.Round((double) level / 3200) * 3200);
                if (step < level) step = level;
                level = step + _featureRockNoise.GetNoise(x, y);
            }
            else
            {
                level = BaseLevel + baseLandscape + level;
            }

            return level < 0 ? 0 : level;
        }
    }
}