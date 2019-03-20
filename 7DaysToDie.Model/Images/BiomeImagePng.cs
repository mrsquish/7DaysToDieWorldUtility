using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeImageAPI;
using _7DaysToDie.Model.Extensions;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDie.Model.Images
{
    public class Biomes : PngImageSquare
    {
        private float _wasteLandPeak = (float)-0.7;
        private float _burntForestPeak = (float)-0.4;
        private float _pineForestPeak = (float)0.3;
        private float _dessertPeak = (float)0.7;        
        //private float _snowPeak = (float)-0.9;

        public Biomes(int size) : base(size)
        {
        }

        public void GenerateBiomes2()
        {
            var myNoise = new FastNoise(46549874);
            myNoise.SetFrequency((float)0.02);
            myNoise.SetGradientPerturbAmp((float)1);
            myNoise.SetInterp(FastNoise.Interp.Hermite);

            for (int i = 0; i < FreeImage.GetHeight(_bitMap); i++)
            {
                _logger.Info($"Generating Line {i}");
                Scanline<RGBTRIPLE> scanline = new Scanline<RGBTRIPLE>(_bitMap, i);
                RGBTRIPLE[] rgbt = scanline.Data;
                for (int j = 0; j < rgbt.Length; j++)
                {
                    float x = i;
                    float y = j;
                    //float z = i;
                    myNoise.GradientPerturb(ref x,  ref y);
                    if (true)
                    {
                        rgbt[j].Color = BaseExtensions.ColorFromHSV(((x - i) + 1) * 180, (y - j).Normalise(), 1);
                    }
                    else
                    {
                        var amplitude = (float)0;
                        x = x - i;
                        y = y - j;
                        if (x > 0.5)
                            amplitude = 50 + x * 200;
                        else 
                        rgbt[j].rgbtBlue = (byte)amplitude;
                        rgbt[j].rgbtGreen = (byte)amplitude;
                        rgbt[j].rgbtRed = (byte)amplitude;
                    }
                }
                scanline.Data = rgbt;
            }
        }


        public void GenerateBiomes()
        {
            var noiseFactory = new NoiseFactory();

            var myNoise = noiseFactory.GetCellularNoiseForBiome((float)0.002);
            for (int i = 0; i < FreeImage.GetHeight(_bitMap); i++)
            {
                _logger.Info($"Generating Line {i}");
                Scanline<RGBTRIPLE> scanline = new Scanline<RGBTRIPLE>(_bitMap, i);
                RGBTRIPLE[] rgbt = scanline.Data;
                for (int j = 0; j < rgbt.Length; j++)
                {
                    var noise = myNoise.GetNoise(i, j);
                    //_logger.Info($"Noise = {noise}");
                    rgbt[j].Color = GetBiomeFromNoise(noise);
                }                
                scanline.Data = rgbt;
            }            
        }

        private Color GetBiomeFromNoise(float noise)
        {
            if (noise < _wasteLandPeak)
                return WasteLandColor;
            if (noise < _burntForestPeak)
                return BurntForestColor;
            if (noise < _pineForestPeak)
                return PineForestColor;
            if (noise < _dessertPeak)
                return DessertColor;
            return SnowColor;
        }

        public readonly Color SnowColor = Color.FromArgb(255, 255, 255);

        public readonly Color PineForestColor = Color.FromArgb(0, 64, 0);

        public readonly Color DessertColor = Color.FromArgb(255, 200, 0);

        public readonly Color WasteLandColor = Color.FromArgb(255, 168, 0);

        public readonly Color BurntForestColor = Color.FromArgb(186, 0, 255);
    }
}
