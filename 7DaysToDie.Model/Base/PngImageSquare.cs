using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeImageAPI;
using NLog;

namespace _7DaysToDie.Model
{
    public class PngImageSquare : IDisposable
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();
        protected FIBITMAP _bitMap;
        private bool _bitMapAllocated = false;

        public PngImageSquare(int size)
        {
            Size = size;
        }

        public void Create()
        {
            _bitMap = FreeImage.Allocate(Size, Size, 32);
        }

        public Scanline<FIRGBF> GetImageLine(int lineNumber)
        {            
            return new Scanline<FIRGBF>(_bitMap, lineNumber);
        }

        public void Save(string fileName)
        {
            var result = FreeImage.SaveEx(ref _bitMap, fileName, FREE_IMAGE_FORMAT.FIF_PNG,
                FREE_IMAGE_SAVE_FLAGS.PNG_Z_DEFAULT_COMPRESSION, FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP, false);
            if (!result)
            {
                _logger.Error($"Saving {fileName} failed.");
            }
        }

        public int Size { get; private set; }

        public void Dispose()
        {
            if (_bitMapAllocated)
                FreeImage.UnloadEx(ref _bitMap);
        }
    }
}
