using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NLog;
using NLog.LayoutRenderers.Wrappers;
using _7DaysToDie.Base;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace _7DaysToDie.Model
{
    public class HeightMap : HeightMap<float>
    {
        public HeightMap(int size) : base(size)
        {
        }

        public void Save(string fileName)
        {
            base.Save(fileName, f => (ushort)f);
        }
        
    }
}