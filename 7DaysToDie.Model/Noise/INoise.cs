using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Noise
{
    public interface INoise
    {
        float GetNoise(float x, float y);
        float Amplitude { get; set; }

        float LastNoise { get; }
    }
}
