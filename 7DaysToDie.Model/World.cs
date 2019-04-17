using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using _7DaysToDie.Model.Contracts;

namespace _7DaysToDie.Model
{
    public class World
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private RgbBitMap _bitMap;
        private bool _bitMapLoaded = false;
        private Prefabs _prefabs;

        private World()
        {
            Biomes = new List<IBiome>();
        }

        public string BasePath { get; set; }
        public List<IBiome> Biomes { get; }
        public string BiomesPngFile { get; set; }

        public WorldSize Size { get; set; } = new WorldSize();

        public Prefabs Prefabs
        {
            get
            {
                if (_prefabs == null)
                    _prefabs = Prefabs.FromFile(Path.Combine(BasePath, "prefabs.xml"));
                return _prefabs;
            }
        }

        public static World LoadWorldPath(string path)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException($"Argument {nameof(path)} must specify a valid path");
            return new World {BasePath = path};
        }
    }
}