using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FreeImageAPI;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDie.Model.Images
{
    public class DesertCanyonBiomeHeightMap : PngImageSquare
    {
        private float _canyonFactor = 50;
        private float _riverFactor = 15;

        public DesertCanyonBiomeHeightMap(int size) : base(size)
        {
        }
        
        public void RegenerateHeightMap()
        {            
            var noiseFactory = new NoiseFactory();
            var featureRockNoise = new FeatureRockNoise(noiseFactory, 5);
            var myNoise = noiseFactory.GetValueFractalForDesertLandscape();
            var cellNoise = noiseFactory.GetCellularNoiseForLandscapeAddition((float) 0.015);
            for (int i = 0; i < Size; i++)
            {
                Scanline<RGBTRIPLE> scanline = new Scanline<RGBTRIPLE>(_bitMap, i);
                RGBTRIPLE[] rgbt = scanline.Data;
                for (int j = 0; j < rgbt.Length; j++)
                {
                    var noise = myNoise.GetNoise(i, j);
                    var levelAdd =  cellNoise.GetNoise(i, j);

                    var grey = GetStandardisedHeight(noise, 50) + 10;

                    if (grey < _riverFactor)
                    {
                        grey = grey + (noise);

                    } else if (grey > _canyonFactor)
                    {
                        grey += (levelAdd * 2 + 4) + featureRockNoise.GetNoise(i, j);
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

        public float GetStandardisedHeight(float noise, float maxHeight)
        {            
            return (noise * (maxHeight/2) + maxHeight);
        }
    }
}
