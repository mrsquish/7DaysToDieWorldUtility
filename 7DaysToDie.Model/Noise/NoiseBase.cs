using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Noise
{
    public abstract class NoiseBase : INoise
    {
        
        private float _amplitude = ((float)ushort.MaxValue);
        private float _middle = ((float) ushort.MaxValue / (float) 2);


        protected NoiseBase()
        {
            
        }
        
        public virtual float Amplitude
        {
            get => _amplitude;
            set
            {
                if (value > ushort.MaxValue)
                    _amplitude = ushort.MaxValue;
                else _amplitude = value;

                _middle = _amplitude / (float) 2;
            }
        }

        public abstract bool Invert { get; set; }

        public abstract float LastNoise { get; }

        public abstract float GetNoise(float x, float y);

        protected float Normalise(float noise)
        {
            return (noise * _middle) + _middle;
        }
    }
}
