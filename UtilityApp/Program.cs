using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FreeImageAPI;
using NLog;
using NLog.Config;
using NLog.Targets;
using _7DaysToDie.Model;
using _7DaysToDie.Model.Images;

namespace _7DaysToDieWorldUtil
{

    // Implement this : https://github.com/Auburns/FastNoise_CSharp
    //http://haishibai.blogspot.com/2009/09/image-processing-c-tutorial-4-gaussian.html
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureLoggingFramework();
            var logger = LogManager.GetCurrentClassLogger();
            // Check if FreeImage.dll is available (can be in %path%).
            if (!FreeImage.IsAvailable())
            {
                logger.Error("FreeImage.dll seems to be missing. Aborting.");
                return;
            }
            BiomeTest();

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
        }

        public static void BiomeTest()
        {
            using (var biomes = new Biomes(1092))
            {
                biomes.Create();
                biomes.GenerateBiomes2();
                biomes.Save(@"C:\Users\ahardy\Documents\HeightMaps\Testing\Biomes.png");
            }
        }

        public static void AdjustPrefabHeight()
        {
            using (var world = World.LoadWorldPath(@"C:\Users\ahardy\Documents\HeightMaps"))
            {
                world.HeightMapPNG = @"C:\Users\ahardy\Documents\HeightMaps\ExampleHeightMap.tga";
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
