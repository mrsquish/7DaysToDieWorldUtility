using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using _7DaysToDie.Model.Contracts;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDie.Model.Biomes
{
    public abstract class BiomeBase : IBiome
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();        
        private readonly Color _baseColor;
        private readonly NoiseFactory _noiseFactory;
        private PngImageSquare _biomePng;

        protected BiomeBase(
            string baseDirectory, 
            Color baseColor,
            int size,
            NoiseFactory noiseFactory)
        {
            BaseDirectory = baseDirectory;
            _baseColor = baseColor;
            _noiseFactory = noiseFactory;
            Size = size;
            if (!Directory.Exists(BaseDirectory))
                Directory.CreateDirectory(BaseDirectory);
        }
        
        public int Size { get; protected set; }
        public string BaseDirectory { get; protected set; }



        public abstract void Generate();
        
        public void Dispose()
        {
            
        }
    }
}
