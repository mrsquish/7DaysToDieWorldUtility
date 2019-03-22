using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Noise
{
    public class FeatureNoise : NoiseBase, INoise
    {
        private readonly NoiseFactory _factory;
        private readonly float _amplitude;
        private INoise _placementRandomiserNoise;
        private INoise _featureNoise;
        private float _lastNoise;
        private float _frequecyThreshold;        

        public FeatureNoise(INoise placementRandomiserNoise, INoise featureNoise) : base()
        {                        
            _placementRandomiserNoise = placementRandomiserNoise;
            _featureNoise = featureNoise;
            _frequecyThreshold = placementRandomiserNoise.Amplitude / 10;
            Amplitude = (float)(ushort.MaxValue / (float)5);
        }

        public override float Amplitude
        {
            get => _featureNoise.Amplitude;
            set => _featureNoise.Amplitude = value;
        }

        public override float LastNoise => _lastNoise;

        public override float GetNoise(float x, float y)
        {
            _lastNoise = _placementRandomiserNoise.GetNoise(x, y);
            if (_lastNoise < _frequecyThreshold)
            {
                return _featureNoise.GetNoise(x, y);
            }
            return 0;
        }
    }

}
