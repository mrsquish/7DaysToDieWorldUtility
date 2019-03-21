using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FreeImageAPI;
using NLog;
using NLog.Config;
using NLog.Targets;
using _7DaysToDie.Model;
using _7DaysToDie.Model.Biomes;
using _7DaysToDie.Model.Images;
using _7DaysToDie.Model.Noise;

namespace _7DaysToDieWorldUtil
{
    // Implement this : https://github.com/Auburns/FastNoise_CSharp
    //http://haishibai.blogspot.com/2009/09/image-processing-c-tutorial-4-gaussian.html
    class Program
    {
        public static string TestingPath;

        static void Main(string[] args)
        {
            if (Directory.Exists("C:\\Users\\ahardy\\Documents\\HeightMaps\\Testing"))
                TestingPath = "C:\\Users\\ahardy\\Documents\\HeightMaps\\Testing";
            else
            {
                TestingPath = "D:\\Program Files (x86)\\Steam\\SteamApps\\common\\7 Days To Die\\Data\\Worlds\\Testing4k";
            }

            ConfigureLoggingFramework();
            var logger = LogManager.GetCurrentClassLogger();
            // Check if FreeImage.dll is available (can be in %path%).
            if (!FreeImage.IsAvailable())
            {
                logger.Error("FreeImage.dll seems to be missing. Aborting.");
                return;
            }
            //BiomeTest();
            HeightMapTest();

            //@"D:\Program Files (x86)\Steam\SteamApps\common\7 Days To Die\Data\Worlds\Testing4k
            //
            /*
            using (var world = World.LoadWorldPath(@"C:\Users\ahardy\Documents\HeightMaps"))
            {
                world.GenerateBiomes();
                //world.RegenerateHeightMap();
                //world.LevelAllPrefabsAtGroundHeight();
                //world.SaveHeightMapPng();
                
            } */
            Console.Read();
        }

        public static void HeightMapTest()
        {
            using (var biome = new DesertCanyonBiome(TestingPath, 1096, new NoiseFactory()))
            {
                biome.Generate();
            }
        }

        public static void BiomeTest()
        {
            using (var biomes = new Biomes(4096))
            {
                biomes.Create();
                biomes.GenerateBiomes();
                biomes.Save(Path.Combine(TestingPath, "Biomes.png"));
            }
        }

        public static void AdjustPrefabHeight()
        {
            using (var world = World.LoadWorldPath(TestingPath))
            {
                world.HeightMapPNG = Path.Combine(TestingPath, "dtm.png");
                world.LevelAllPrefabsAtGroundHeight();
                world.Prefabs.Save();
            }
        }
        
        public static void ConfigureLoggingFramework()
        {
            LogManager.LoadConfiguration("nlog.config");
        }
    }
}
