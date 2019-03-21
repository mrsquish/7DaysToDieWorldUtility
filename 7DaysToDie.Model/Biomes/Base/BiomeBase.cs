using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDie.Model.Biomes
{
    public abstract class BiomeBase : IDisposable
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _baseDirectory;
        private readonly Color _baseColor;
        private readonly NoiseFactory _noiseFactory;
        
        protected BiomeBase(
            string baseDirectory, 
            Color baseColor,
            int size,
            NoiseFactory noiseFactory)
        {
            _baseDirectory = baseDirectory;
            _baseColor = baseColor;
            _noiseFactory = noiseFactory;
            Size = size;
        }

        protected PngImageSquare HeightMap { get; set; }
        protected int Size { get; set; }

        public abstract void Generate();

        public virtual void Create()
        {
            if (!Directory.Exists(_baseDirectory))
                Directory.CreateDirectory(_baseDirectory);

        }

        public void Dispose()
        {
            HeightMap?.Dispose();
        }
    }
}
