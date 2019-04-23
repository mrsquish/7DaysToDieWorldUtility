using System;
using System.IO;
using NLog;
using _7DaysToDie.Maze;
using _7DaysToDie.Model.Biomes;
using _7DaysToDie.Model.Noise;

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


    /// Path Finding :
    /// https://blogs.msdn.microsoft.com/ericlippert/tag/astar/
    /// https://blogs.msdn.microsoft.com/ericlippert/2007/10/04/path-finding-using-a-in-c-3-0-part-two/
    internal class Program
    {
        public static string TestingPath;


        private static void Main(string[] args)
        {
            if (Directory.Exists("C:\\Users\\ahardy\\Documents\\HeightMaps\\Testing"))
                TestingPath = "C:\\Users\\ahardy\\Documents\\HeightMaps\\Testing";
            else if (Directory.Exists(
                "D:\\Program Files (x86)\\Steam\\SteamApps\\common\\7 Days To Die\\Data\\Worlds\\Testing4k"))
                TestingPath =
                    "D:\\Program Files (x86)\\Steam\\SteamApps\\common\\7 Days To Die\\Data\\Worlds\\Testing4k";
            else
                TestingPath = @"C:\Users\adam\AppData\Roaming\7DaysToDie\GeneratedWorlds\Testing";


            ConfigureLoggingFramework();
            var logger = LogManager.GetCurrentClassLogger();

            HeightMapTest();
            return;

            TestMaze();
            return;
        }

        public static void TestMaze()
        {
            var maze = new Maze(128);
            maze.GenerateRecursiveBackTracker(15);
            maze.RenderToHeightMap(Path.Combine(TestingPath, "maze.raw"), 32);
        }

        public static void HeightMapTest()
        {
            using (var biome = new TestBiome(TestingPath, 1096, new NoiseFactory(555)))// new NoiseFactory(DateTime.Now.Millisecond)))
            {
                biome.Generate();
            }
        }

        public static void ConfigureLoggingFramework()
        {
            LogManager.LoadConfiguration("nlog.config");
        }
    }
}