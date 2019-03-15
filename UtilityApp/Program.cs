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

namespace _7DaysToDieWorldUtil
{
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
            
            using (var world = World.LoadWorldPath(@"C:\Users\ahardy\Documents\HeightMaps"))
            {
                world.HeightMapTga = @"C:\Users\ahardy\Documents\HeightMaps\ExampleHeightMap.tga";
                world.LevelAllPrefabsAtGroundHeight();
                world.Prefabs.Save();
            }
            /*
            // Store the bitmap to disk
            if (!FreeImage.SaveEx(ref dib, "SampleOut02.jpg", FREE_IMAGE_SAVE_FLAGS.DEFAULT, true))
            {
                Console.WriteLine("Error while saving 'SampleOut02.jpg'");
                FreeImage.UnloadEx(ref dib);
            }
            */

            Console.Read();

        }
        
        public static void ConfigureLoggingFramework()
        {
            LogManager.LoadConfiguration("nlog.config");
        }
    }
}
