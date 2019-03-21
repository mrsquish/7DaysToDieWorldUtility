using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Extensions
{
    public static class BaseExtensions
    {

        public static float Normalise(this float noiseFloat)
        {
            return (noiseFloat + 1) / 2;
        }

        public static float ToRGB(this float noiseFloat)
        {
            return noiseFloat.Normalise() * 255;
        }

        public static Color BiomeFromHSV(double hue, double saturation, double value)
        {
            if (value > 1 || value < 0)
                throw new ArgumentException("Parameter is outside bounds", nameof(value));
            if (hue > 360 || hue < 0)
                throw new ArgumentException("Parameter is outside bounds", nameof(hue));


            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Biome.SnowColor;
            else if (hi == 1)
                return Biome.BurntForestColor;
            else if (hi == 2)
                return Biome.DesertColor;
            else if (hi == 3)
            {
                return Biome.PineForestColor;
                //return Color.Black;
                return Color.FromArgb(255, p, q, v);
            }
            else if (hi == 4)
            {
                return Biome.WasteLandColor;
                return Color.FromArgb(255, t, p, v);
            }
            else
            {
                return Biome.PineForestColor;
                return Color.FromArgb(255, v, p, q);
            }

        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            if (value > 1 || value < 0)
                throw new ArgumentException("Parameter is outside bounds", nameof(value));
            if (hue > 360 || hue < 0)
                throw new ArgumentException("Parameter is outside bounds", nameof(hue));


            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);                
            else if (hi == 3)
            {
                //return Color.Black;
                return Color.FromArgb(255, p, q, v);
            }
            else if (hi == 4)
            {
                return Color.Black;
                return Color.FromArgb(255, t, p, v);
            }
            else
            {
                return Color.Black;
                return Color.FromArgb(255, v, p, q);
            }
                
        }
    }
}
