using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Mime;
using NLog;
using _7DaysToDie.Model.Contracts;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDie.Model
{
    public class World : IDisposable
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private Prefabs _prefabs;
        private FIBITMAP _bitMap = new FIBITMAP();        
        private bool _bitMapLoaded = false;

        public string BasePath { get; set; }
        public List<IBiome> Biomes { get; private set; }
        public string BiomesPngFile { get; set; }

        public WorldSize Size { get; set; } = new WorldSize();

        public static World LoadWorldPath(string path)
        {
            if (!Directory.Exists(path))
                throw  new ArgumentException($"Argument {nameof(path)} must specify a valid path");
            return new World() {BasePath = path};
        }

        private World()
        {
            Biomes = new List<IBiome>();
        }

        public Prefabs Prefabs
        {
            get
            {
                if (_prefabs == null)
                    _prefabs = Prefabs.FromFile(Path.Combine(BasePath, "prefabs.xml"));
                return _prefabs;
            }
        }

        public uint Height {
            get
            {
                return FreeImage.GetHeight(_bitMap);
            }
        }

        public void LevelAllPrefabsAtGroundHeight()
        {
            foreach (var prefab in Prefabs.Decorations)
            {
                /*var newHeight = GetGroundLevelAtPrefab(prefab);
                _logger.Info($"Prefab [{prefab.Name}] { (newHeight > prefab.Position.Y ? "raised" : "lowered") } from { prefab.Position.Y } to {newHeight } ");
                prefab.Position.Y = newHeight;*/
            }
        }

        public FIBITMAP LoadBiomesPng()
        {
            BiomesPngFile = Path.Combine(BasePath, "biomes.png"); 
            return FreeImage.Load(FREE_IMAGE_FORMAT.FIF_PNG, BiomesPngFile, FREE_IMAGE_LOAD_FLAGS.PNG_IGNOREGAMMA);            
        }
        public void SaveBiomesPng(FIBITMAP bitMap)
        {
            if (!FreeImage.SaveEx(bitMap, BiomesPngFile, FREE_IMAGE_FORMAT.FIF_PNG))
            {
                _logger.Error($"Saving {BiomesPngFile} failed.");
            }
            FreeImage.UnloadEx(ref bitMap);
        }
        
        public void Dispose()
        {
            if (_bitMapLoaded)
                FreeImage.UnloadEx(ref _bitMap);
        }
    }
}
