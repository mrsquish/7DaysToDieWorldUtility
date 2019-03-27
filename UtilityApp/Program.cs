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
    /// Biome Ideas :
    /// Swamp - this could be used in "green" or "wasteland" or "burnt"

    ///Good Erosion explanation and source code(CPP)
    ///http://ranmantaru.com/blog/2011/10/08/water-erosion-on-heightmap-terrain/


    // Implement this : https://github.com/Auburns/FastNoise_CSharp
    // Gausian Blur if needed : http://haishibai.blogspot.com/2009/09/image-processing-c-tutorial-4-gaussian.html
    class Program
    {
        public static string TestingPath;

        

        static void Main(string[] args)
        {
            /*
            PngImageSquare.TestPng();
            return;
            */
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
            HeightMapTest();

        }

        public static void HeightMapTest()
        {
            using (var biome = new TestBiome(TestingPath, 4096, new NoiseFactory()))
            {                
                biome.Generate();
            }
        }

        public static void AdjustPrefabHeight()
        {
            using (var world = World.LoadWorldPath(TestingPath))
            {                
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
