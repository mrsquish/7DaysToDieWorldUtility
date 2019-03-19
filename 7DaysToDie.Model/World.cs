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

        public byte GetStandardisedHeight(float noise)
        {
            var ZerBasedNoise = noise + 1;
            return (byte)((int) (ZerBasedNoise * 60 + 20));
        }

        public void GenerateBiomes()
        {
            var bitMap = LoadBiomesPng();
            var noiseFactory = new NoiseFactory();

            var myNoise = noiseFactory.GetCellularNoiseForBiome();
            for (int i = 0; i < FreeImage.GetHeight(bitMap); i++)
            {
                Scanline<RGBTRIPLE> scanline = new Scanline<RGBTRIPLE>(bitMap, i);
                RGBTRIPLE[] rgbt = scanline.Data;
                for (int j = 0; j < rgbt.Length; j++)
                {
                    var noise = myNoise.GetNoise(i, j);
                    /*
                    if (noise < -0.1)
                    {
                        rgbt[j].rgbtBlue = 255;
                        rgbt[j].rgbtGreen = 0;
                        rgbt[j].rgbtRed = 186;
                    }
                    else if (noise < -0.4)
                    {
                        rgbt[j].rgbtBlue = 0;
                        rgbt[j].rgbtGreen = 168;
                        rgbt[j].rgbtRed = 255;                        
                    }
                    else if (noise < 1.1)
                    {
                        rgbt[j].rgbtBlue = 0;
                        rgbt[j].rgbtGreen = 64;
                        rgbt[j].rgbtRed = 0;
                    }
                    else
                    {
                        rgbt[j].rgbtBlue = 255;
                        rgbt[j].rgbtGreen = 255;
                        rgbt[j].rgbtRed = 255;
                    } */                       
                    rgbt[j].Color = GetBiomeFromNoise(noise);
                    
                }
                _logger.Info($"Writing Line {i}");
                scanline.Data = rgbt;
            }
            SaveBiomesPng(bitMap);
        }


        private Color GetBiomeFromNoise(float noise)
        {
            if (noise < -0.1)
                return BurntForestColor();
            if (noise < -0.4)
                return WasteLandColor();
            if (noise < 1.1)
                return PineForestColor();
            return SnowColor();
        }

        private Color SnowColor() => Color.FromArgb(255, 255, 255);

        private Color PineForestColor() => Color.FromArgb(0, 64, 0);

        public Color WasteLandColor() => Color.FromArgb(255, 168, 0);

        public Color BurntForestColor() => Color.FromArgb(186, 0, 255);


        public void RegenerateHeightMap()
        {
            LoadHeightMapPng();
            var noiseFactory = new NoiseFactory();
            /* Value noise type with .005 frequency = sharp (square) rolling hills.
             * Value Fractal with 0.005 freq = more natural hills, less frequency would be better.
             * Simplex with 0.001 freq = rolling hills.
             */

            var myNoise = noiseFactory.GetCellularNoiseForBiome();
            for (int i = 0; i < Height; i++)
            {
                Scanline<RGBTRIPLE> scanline = new Scanline<RGBTRIPLE>(_bitMap, i);
                RGBTRIPLE[] rgbt = scanline.Data;
                for (int j = 0; j < rgbt.Length; j++)
                {
                    var noise = myNoise.GetNoise(i, j);
                    
                    var grey = GetStandardisedHeight(noise);
                    rgbt[j].rgbtBlue = grey;
                    rgbt[j].rgbtGreen = grey;
                    rgbt[j].rgbtRed = grey;
                    
                }
                _logger.Info($"Writing Line {i}");
                scanline.Data = rgbt;
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
