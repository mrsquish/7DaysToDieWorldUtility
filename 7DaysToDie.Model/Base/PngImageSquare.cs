using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeImageAPI;
using NLog;

namespace _7DaysToDie.Model
{
    public abstract class PngImageSquare : IDisposable
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();
        protected FIBITMAP _bitMap;
        private bool _bitMapAllocated = false;

        protected PngImageSquare(int size)
        {
            Size = size;
        }

        public void Create()
        {
            _bitMap = FreeImage.Allocate(Size, Size, 24);
        }

        public Scanline<RGBTRIPLE> GetImageLine(int lineNumber)
        {
            return new Scanline<RGBTRIPLE>(_bitMap, lineNumber);
        }

        public void Save(string fileName)
        {
            if (!FreeImage.SaveEx(_bitMap, fileName, FREE_IMAGE_FORMAT.FIF_PNG, FREE_IMAGE_SAVE_FLAGS.PNG_Z_DEFAULT_COMPRESSION))
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
