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

        public ushort[] _bitMap;
        private byte[] _bytes;

        private Bitmap _bitmap2;
        
        public PngImageSquare(int size)
        {
            Size = size;
            _bytes = new byte[Size * Size * 2];
        }

        public int Size { get; }

        public void Dispose()
        {
        }

        public static void TestPng()
        {
            var bitmap = new byte[10 * 10 * 2];
            bitmap[0] = 255;
            bitmap[4] = 255;
            bitmap[8] = 255;
            using (Image image = Image.FromStream(new MemoryStream(bitmap)))
            {
                image.Save("output.jpg", ImageFormat.Jpeg);  // Or Png
            }

        }

        public void Create()
        {

            Bitmap cur = new Bitmap(Size, Size, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            BitmapData curBitmapData = cur.LockBits(new Rectangle(0, 0, Size, Size),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            Int32 stride = curBitmapData.Stride;
            Byte[] data = new Byte[stride * cur.Height];
            _bytes = new Byte[stride * cur.Height];

            Marshal.Copy(curBitmapData.Scan0, data, 0, data.Length);
            cur.UnlockBits(curBitmapData);

            _bitMap = new ushort[Size * Size];

            //_bitmap2 = new Bitmap(Size,Size, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
        }

        public void SavePng(string fileName)
        {
            var rect = new Rectangle(0, 0, Size, Size);
            var b16bpp = new Bitmap(Size, Size, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            var bitmapData = b16bpp.LockBits(rect, ImageLockMode.WriteOnly, b16bpp.PixelFormat);
            
            // Copy the randomized bits to the bitmap pointer.
            var ptr = bitmapData.Scan0;
            Copy(_bitMap, ptr, 0, _bitMap.Length);

            // Unlock the bitmap, we're all done.
            b16bpp.UnlockBits(bitmapData);
            SaveBmp(b16bpp, fileName);
        }

        public void SetPixel(int x, int y, Color color)
        {
            var i = ((y * Size) + x);
            
        }

        [DllImport("kernel32.dll", SetLastError = false)]
        private static extern void CopyMemory(IntPtr destination, IntPtr source, UIntPtr length);

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

        public static void SaveBmp(Bitmap bmp, string path)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

            var pixelFormats = ConvertBmpPixelFormat(bmp.PixelFormat);

            var source = BitmapSource.Create(bmp.Width,
                bmp.Height,
                bmp.HorizontalResolution,
                bmp.VerticalResolution,
                pixelFormats,
                null,
                bitmapData.Scan0,
                bitmapData.Stride * bmp.Height,
                bitmapData.Stride);

            bmp.UnlockBits(bitmapData);


            var stream = new FileStream(path, FileMode.Create);
            var encoder = new PngBitmapEncoder() { };
            //var encoder = new TiffBitmapEncoder {Compression = TiffCompressOption.Zip};

            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);

            stream.Close();
        }

        private static System.Windows.Media.PixelFormat ConvertBmpPixelFormat(System.Drawing.Imaging.PixelFormat pixelformat)
        {
            var pixelFormats = PixelFormats.Default;

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
