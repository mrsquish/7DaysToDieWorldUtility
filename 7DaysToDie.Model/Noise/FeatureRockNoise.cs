using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Noise
{
    public class FeatureRockNoise : INoise
    {
        private readonly NoiseFactory _factory;
        private readonly float _amplitude;
        private FastNoise _placementRandomiserNoise;
        private FastNoise _featureNoise;

        public FeatureRockNoise(NoiseFactory factory, float amplitude)
        {
            _factory = factory;
            _amplitude = amplitude;
            _placementRandomiserNoise = _factory.GetCellularNoiseForLandscapeAddition((float) 0.02);
            _featureNoise = _factory.GetCellularNoiseForRockFeatures();
        }

        public float GetNoise(float x, float y)
        {
            var height = _placementRandomiserNoise.GetNoise(x, y);
            if (height < -0.6)
            {
                return _featureNoise.GetNoise(x, y) * (_amplitude / 2) + _amplitude;
            }
            return 0;
        }
    }
}
