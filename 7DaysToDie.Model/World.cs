using FreeImageAPI;
using System;
using System.Drawing;
using System.IO;
using System.Net.Mime;
using NLog;
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
        public string HeightMapPNG { get; set; }
        public string BiomesPNGFile { get; set; }

        public WorldSize Size { get; set; } = new WorldSize();

        public static World LoadWorldPath(string path)
        {
            if (!Directory.Exists(path))
                throw  new ArgumentException($"Argument {nameof(path)} must specify a valid path");
            return new World() {BasePath = path};
        }

        private World()
        {
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

        public FIBITMAP BitMap 
        {
            get
            {
                if (!_bitMapLoaded )
                {
                    LoadHeightMapPng(HeightMapPNG);
                }

                return _bitMap;
            }
        }

        public Scanline<RGBTRIPLE> GetScanlineForPrefab(Decoration prefab)
        {
            return new Scanline<RGBTRIPLE>(BitMap, prefab.Position.ToZeroBased(Size).Z);
        }

        public int GetGroundLevelAtPrefab(Decoration prefab)
        {
            Scanline<RGBTRIPLE> scanline = GetScanlineForPrefab(prefab);
            var scanLineBitMap = scanline.Data;
            return scanLineBitMap[prefab.Position.ToZeroBased(Size).X].rgbtBlue;
        }

        public void LevelAllPrefabsAtGroundHeight()
        {
            foreach (var prefab in Prefabs.Decorations)
            {
                var newHeight = GetGroundLevelAtPrefab(prefab);
                _logger.Info($"Prefab [{prefab.Name}] { (newHeight > prefab.Position.Y ? "raised" : "lowered") } from { prefab.Position.Y } to {newHeight } ");
                prefab.Position.Y = newHeight;
            }
        }

        public void LoadHeightMapPng(string filePath = "")
        {
            if (string.IsNullOrWhiteSpace(filePath))
                filePath = Path.Combine(BasePath, "dtm.png");
            HeightMapPNG = filePath;
            _bitMap = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_PNG, filePath, FREE_IMAGE_LOAD_FLAGS.PNG_IGNOREGAMMA);
            _bitMapLoaded = true;
            Size = WorldSize.FromImageHeight(FreeImage.GetHeight(_bitMap));
        }

        public FIBITMAP LoadBiomesPng()
        {
            BiomesPNGFile = Path.Combine(BasePath, "biomes.png"); 
            return FreeImage.Load(FREE_IMAGE_FORMAT.FIF_PNG, BiomesPNGFile, FREE_IMAGE_LOAD_FLAGS.PNG_IGNOREGAMMA);            
        }
        public void SaveBiomesPng(FIBITMAP bitMap)
        {
            if (!FreeImage.SaveEx(bitMap, BiomesPNGFile, FREE_IMAGE_FORMAT.FIF_PNG))
            {
                _logger.Error($"Saving {BiomesPNGFile} failed.");
            }
            FreeImage.UnloadEx(ref bitMap);
        }

        public void SaveHeightMapPng()
        {
            if (!FreeImage.SaveEx(BitMap, HeightMapPNG, FREE_IMAGE_FORMAT.FIF_PNG))
            {
                _logger.Error($"Saving {HeightMapPNG} failed.");                
            }
            
        }


        public void Dispose()
        {
            if (_bitMapLoaded)
                FreeImage.UnloadEx(ref _bitMap);
        }
    }
}
