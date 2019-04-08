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
using _7DaysToDie.Maze;
using _7DaysToDie.Model;
using _7DaysToDie.Model.Biomes;
using _7DaysToDie.Model.Images;
using _7DaysToDie.Model.Noise;
using _7DaysToDie.Roads;

namespace _7DaysToDieWorldUtil
{
    /// Biome Ideas :
    /// Swamp - this could be used in "green" or "wasteland" or "burnt"

    ///Good Erosion explanation and source code(CPP)
    ///http://ranmantaru.com/blog/2011/10/08/water-erosion-on-heightmap-terrain/

    ///https://github.com/SebLague/Hydraulic-Erosion
    ///
    /// https://firespark.de/?id=article&article=ProceduralGenerationResources

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
            else if (Directory.Exists("D:\\Program Files (x86)\\Steam\\SteamApps\\common\\7 Days To Die\\Data\\Worlds\\Testing4k"))
            {
                TestingPath = "D:\\Program Files (x86)\\Steam\\SteamApps\\common\\7 Days To Die\\Data\\Worlds\\Testing4k";
            } else
            {
                TestingPath = @"C:\Users\adam\AppData\Roaming\7DaysToDie\GeneratedWorlds\Testing";
            }

            TestPng();
            return;

            TestMaze();
            return;

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

        private static void TestPng()
        {
            var roads = new RoadGenerator();
            roads.Generate(Path.Combine(TestingPath, "roads.png"));
        }

        public static void TestMaze()
        {
            var maze = new Maze(64);
            maze.GenerateRecursiveBackTracker();
            maze.RenderToHeightMap(Path.Combine(TestingPath, "maze.raw"), 8);

        }

        public static void HeightMapTest()
        {
            using (var biome = new TestBiome(TestingPath, 1096, new NoiseFactory()))
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
