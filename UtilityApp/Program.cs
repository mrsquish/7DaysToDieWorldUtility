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
                Console.WriteLine("FreeImage.dll seems to be missing. Aborting.");
                return;
            }

            var prefabs = Prefabs.FromFile(@"C:\Users\ahardy\Documents\HeightMaps\prefabs.xml");
            
            const string fileName = @"C:\Users\ahardy\Documents\HeightMaps\ExampleHeightMap.tga";            
            
            var dib = new FIBITMAP();
            dib = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_TARGA, fileName, FREE_IMAGE_LOAD_FLAGS.TARGA_LOAD_RGB888);
            var worldSize = WorldSize.FromImageHeight(FreeImage.GetHeight(dib));

            foreach (var prefab in prefabs.Decorations)
            {
                Scanline<RGBTRIPLE> scanline = new Scanline<RGBTRIPLE>(dib, prefab.Position.ToZeroBased(worldSize).Z);
                var scanLineBitMap = scanline.Data;
                var newHeight = scanLineBitMap[prefab.Position.ToZeroBased(worldSize).X].rgbtBlue;
                logger.Info($"Prefab [{prefab.Name}] { (newHeight > prefab.Position.Y ? "raised" : "lowered") } from { prefab.Position.Y } to {newHeight } ");
                prefab.Position.Y = newHeight;                                
            }
            prefabs.Save();
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
            /*var consoleTarget = new ConsoleTarget("StandardOutput");
            LoggingRule rule = new LoggingRule("*", NLog.LogLevel.Trace, consoleTarget);

            LogManager.Configuration.AddTarget(consoleTarget);
            //_log.Configuration.LoggingRules.Add(rule);
            */
        }
    }
}
