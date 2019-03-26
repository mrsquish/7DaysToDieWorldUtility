using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Noise
{
    public class GeneralNoise : NoiseBase
    {
        private readonly FastNoise _noiseGenerator;
        private float _lastNoise;
        private int _invert = 1;


        public GeneralNoise(FastNoise noiseGenerator)
        {
            _noiseGenerator = noiseGenerator;
        }

        public override float LastNoise => _lastNoise;

        public override bool Invert
        {
            get => _invert==-1;
            set => _invert = value? -1 : 1;
        }

        public override float GetNoise(float x, float y)
        {
            _lastNoise = _noiseGenerator.GetNoise(x, y) * _invert;
            return Normalise(_lastNoise);
        }
    }
}
