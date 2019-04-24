using System;
using System.IO;
using _7DaysToDie.Model.Extensions;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDie.Model.Biomes
{
    public class SwampBiome : BiomeBase
    {
        private readonly INoise _cellNoise;

        private readonly INoise _featureRockNoise;
        private readonly INoise _generalRollingBaseNoise;
        private readonly INoise _swampFillerNoise;
        private readonly INoise _swampPathNoise;

        private float _canyonFactor;
        private float _cellNoiseMultiplier;
        private float _pathLevel;
        private float _stepFactor;

        public SwampBiome(string baseDirectory, int size, NoiseFactory noiseFactory)
            : base(Path.Combine(baseDirectory, nameof(TestBiome)),
                Biome.DesertColor, size, noiseFactory)
        {
            _generalRollingBaseNoise = noiseFactory.GetRollingBaseLandscape((float) 0.003);
            _swampPathNoise = noiseFactory.GetPerlinFractalBillow((float) 0.02);
            _swampFillerNoise = noiseFactory.GetPerlinHermite((float) 0.04);
            _swampPathNoise.Invert = true;

            _featureRockNoise = noiseFactory.GetFeatureRockNoise((float) 0.06, (float) 0.1);
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
            _stepFactor = 1200;
            _swampPathNoise.Amplitude = (float) ushort.MaxValue / 20;
            _swampFillerNoise.Amplitude = _stepFactor;
            _pathLevel = _swampPathNoise.Amplitude - _stepFactor;

            _generalRollingBaseNoise.Amplitude = (float) ushort.MaxValue / 6;

            _cellNoise.Amplitude = 8; // (float)ushort.MaxValue / 20;
            _cellNoiseMultiplier = (float) ushort.MaxValue / 30;

            _featureRockNoise.Amplitude = (float) ushort.MaxValue / 15;

            _canyonFactor = _swampPathNoise.Amplitude - _swampPathNoise.Amplitude / (float) 1.5;
        }

        public void RegenerateHeightMap(HeightMap heightMap)
        {
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++) heightMap.SetPixel(x, y, GetPixelShade(x, y));
                _logger.Info($"Writing Line {y}");
            }
        }

        private ushort GetPixelShade(int x, int y)
        {
            //float baseLandscape = _generalRollingBaseNoise.GetNoise(x, y);
            var level = _swampPathNoise.GetNoise(x, y);

            level = BaseLevel + (float) (Math.Round((double) level / _stepFactor) * _stepFactor);

            if (level < _pathLevel) level = _pathLevel - _swampFillerNoise.Amplitude + _swampFillerNoise.GetNoise(x, y);
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