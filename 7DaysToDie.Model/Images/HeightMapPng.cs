using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeImageAPI;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDie.Model.Images
{
    public class HeightMapPng : PngImageSquare
    {
        public HeightMapPng(int size) : base(size)
        {
        }
        
        public void RegenerateHeightMap()
        {            
            var noiseFactory = new NoiseFactory();
            /* Value noise type with .005 frequency = sharp (square) rolling hills.
             * Value Fractal with 0.005 freq = more natural hills, less frequency would be better.
             * Simplex with 0.001 freq = rolling hills.
             */
            var myNoise = noiseFactory.GetValueFractalForLandscape2();
            var cellNoise = noiseFactory.GetCellularNoiseForLandscapeAddition((float) 0.02);
            for (int i = 0; i < Size; i++)
            {
                Scanline<RGBTRIPLE> scanline = new Scanline<RGBTRIPLE>(_bitMap, i);
                RGBTRIPLE[] rgbt = scanline.Data;
                for (int j = 0; j < rgbt.Length; j++)
                {
                    float x = j;
                    float y = i;
                    //float z = i;
                    //myNoise.GradientPerturbFractal(ref x, ref y);

                    var noise = myNoise.GetNoise(i, j);
                    var levelAdd =  cellNoise.GetNoise(i, j);

                    var grey = GetStandardisedHeight(noise);
                    
                    if (grey > 60)
                    {
                        grey += (levelAdd*10+20);
                    }

                    var greyByte = (byte) grey;
                    rgbt[j].rgbtBlue = greyByte;
                    rgbt[j].rgbtGreen = greyByte;
                    rgbt[j].rgbtRed = greyByte;
                }
                _logger.Info($"Writing Line {i}");
                scanline.Data = rgbt;
            }
        }

        public float GetStandardisedHeight(float noise)
        {            
            return (noise * 30 + 80);
        }
    }
}
