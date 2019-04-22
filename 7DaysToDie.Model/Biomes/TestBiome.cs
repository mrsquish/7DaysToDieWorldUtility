using _7DaysToDie.Base;
using _7DaysToDie.Model.Extensions;
using _7DaysToDie.Model.Noise;
using _7DaysToDie.Roads;
using System.IO;

namespace _7DaysToDie.Model.Biomes
{
    public class TestBiome : BiomeBase
    {
        private readonly INoise _featureRockNoise;
        private readonly INoise _generalLandscapeNoise;
        private readonly INoise _generalRollingBaseNoise;
        private readonly INoise _cellNoise;

        public TestBiome(string baseDirectory, int size, NoiseFactory noiseFactory)
            : base(Path.Combine(baseDirectory, nameof(TestBiome)),
                Biome.DesertColor, size, noiseFactory)
        {
            _generalRollingBaseNoise = noiseFactory.GetRollingBaseLandscape((float)0.002);
            _featureRockNoise = noiseFactory.GetFeatureRockNoise((float)0.06, (float)0.1);
            
            _generalLandscapeNoise = noiseFactory.GetPerlinFractalBillow((float)0.004);
            _cellNoise = noiseFactory.GetRollingBaseLandscape((float)0.05); //noiseFactory.GetCellularNoiseForLandscapeAddition((float)0.015);
        }

        public override void Generate()
        {
            using (var heightMap = new HeightMap(Size))
            {
                SetLevels();
                heightMap.Create();
                RegenerateHeightMap(heightMap);
                GenerateRoads(heightMap);
                //Erode(heightMap);

                heightMap.Save(Path.Combine(BaseDirectory, "dtm.raw"));
            }
        }

        private void GenerateRoads(HeightMap heightMap)
        {
            var generator = new RoadGenerator(2, heightMap);
            generator.Generate();
            generator.Save(BaseDirectory);
        }

        private void Erode(HeightMap heightMap)
        {
            var erosion = new _7DaysToDie.Erosion.Erosion();
            erosion.Erode(heightMap, heightMap.Size, 1000000, true);
        }

        private void SetLevels()
        {
            
            _generalRollingBaseNoise.Amplitude = WorldSettings.UnitLevel * 40;
            _generalLandscapeNoise.Amplitude = WorldSettings.UnitLevel * 120;

            _cellNoise.Amplitude = (float)8;

            _featureRockNoise.Amplitude = (float)ushort.MaxValue / 15;            
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

            level = WorldSettings.GroundLevel + baseLandscape + level;
            
            return level < 0 ? 0 : level;
        }



    }
}