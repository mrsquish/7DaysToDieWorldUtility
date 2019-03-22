using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FreeImageAPI;
using NLog;
using Color = System.Drawing.Color;

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
            _bitMap = FreeImage.AllocateT(FREE_IMAGE_TYPE.FIT_RGB16, Size, Size, 48);
        }

        public IntPtr GetScanLine(int lineNumber)
        {
            return FreeImage.GetScanLine(_bitMap, lineNumber);
        }

        public Scanline<FIRGBAF> GetImageLine(int lineNumber)
        {            
            return new Scanline<FIRGBAF>(_bitMap, lineNumber);
        }

        public FIRGBAF GetColor(Color color)
        {
            return new FIRGBAF(color);
        }

        public void Save(string fileName)
        {
            var result = FreeImage.SaveEx(ref _bitMap, fileName, 
                FREE_IMAGE_FORMAT.FIF_PNG,
                FREE_IMAGE_SAVE_FLAGS.PNG_Z_DEFAULT_COMPRESSION, 
                FREE_IMAGE_COLOR_DEPTH.FICD_FORCE_GREYSCALE, false);
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

        public  static void SaveBmp(Bitmap bmp, string path)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            BitmapData bitmapData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

            var pixelFormats = ConvertBmpPixelFormat(bmp.PixelFormat);

            BitmapSource source = BitmapSource.Create(bmp.Width,
                                                      bmp.Height,
                                                      bmp.HorizontalResolution,
                                                      bmp.VerticalResolution,
                                                      pixelFormats,
                                                      null,
                                                      bitmapData.Scan0,
                                                      bitmapData.Stride * bmp.Height,
                                                      bitmapData.Stride);

            bmp.UnlockBits(bitmapData);


            FileStream stream = new FileStream(path, FileMode.Create);

            TiffBitmapEncoder encoder = new TiffBitmapEncoder();

            encoder.Compression = TiffCompressOption.Zip;
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);

            stream.Close();
        }

        [DllImport("kernel32.dll", SetLastError = false)]
        static extern void CopyMemory(IntPtr destination, IntPtr source, UIntPtr length);

        public static void Copy<T>(T[] source, IntPtr destination, int startIndex, int length)
            where T : struct
        {
            var gch = GCHandle.Alloc(source, GCHandleType.Pinned);
            try
            {
                var sourcePtr = Marshal.UnsafeAddrOfPinnedArrayElement(source, startIndex);
                var bytesToCopy = Marshal.SizeOf(typeof(T)) * length;

                CopyMemory(destination, sourcePtr, (UIntPtr) bytesToCopy);
            }
            finally
            {
                gch.Free();
            }
        }

        private Bitmap b16bpp;
        public void GenerateDummy16bitImage(string fileName)
        {
            b16bpp = new Bitmap(Size, Size, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);

            // Calculate the number of bytes required and allocate them.            
            var bitmapBytes = new ushort[Size * Size];
            // Fill the bitmap bytes with random data.
            var random = new Random();
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {

                    var i = ((y * Size) + x); // 16bpp

                    // Generate the next random pixel color value.
                    var value = (ushort)ushort.MaxValue;

                    bitmapBytes[i] = value;         // GRAY
                }
            }

            var rect = new Rectangle(0, 0, Size, Size);
            var bitmapData = b16bpp.LockBits(rect, ImageLockMode.WriteOnly, b16bpp.PixelFormat);
            // Copy the randomized bits to the bitmap pointer.
            var ptr = bitmapData.Scan0;
            Copy(bitmapBytes, ptr, 0, bitmapBytes.Length);
            //Marshal.Copy(bitmapBytes, 0, ptr, bitmapBytes.Length);

            // Unlock the bitmap, we're all done.
            b16bpp.UnlockBits(bitmapData);
            SaveBmp(b16bpp, fileName);
            
            _logger.Info("GenerateDummy16bitImage - saved");
        }

        private static System.Windows.Media.PixelFormat ConvertBmpPixelFormat(System.Drawing.Imaging.PixelFormat pixelformat)
        {
            System.Windows.Media.PixelFormat pixelFormats = System.Windows.Media.PixelFormats.Default;

            switch (pixelformat)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    pixelFormats = PixelFormats.Bgr32;
                    break;

                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    pixelFormats = PixelFormats.Gray8;
                    break;

                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    pixelFormats = PixelFormats.Gray16;
                    break;
            }

            return pixelFormats;
        }
    }
}
