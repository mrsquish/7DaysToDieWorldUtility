using FreeImageAPI;
using System;
using System.IO;
using NLog;

namespace _7DaysToDie.Model
{
    public class World : IDisposable
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private Prefabs _prefabs;
        private FIBITMAP _bitMap = new FIBITMAP();
        private bool _bitMapLoaded = false;

        public string BasePath { get; set; }
        public string HeightMapTga { get; set; }

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

        public FIBITMAP BitMap 
        {
            get
            {
                if (!_bitMapLoaded )
                {
                    LoadHeightMapTga(HeightMapTga);
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

        public void LoadHeightMapTga(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                filePath = Path.Combine(BasePath, "dtm.tga");
            HeightMapTga = filePath;
            _bitMap = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_TARGA, filePath, FREE_IMAGE_LOAD_FLAGS.TARGA_LOAD_RGB888);
            _bitMapLoaded = true;
            Size = WorldSize.FromImageHeight(FreeImage.GetHeight(_bitMap));
        }

        public void Dispose()
        {
            if (_bitMapLoaded)
                FreeImage.UnloadEx(ref _bitMap);
        }
    }
}
