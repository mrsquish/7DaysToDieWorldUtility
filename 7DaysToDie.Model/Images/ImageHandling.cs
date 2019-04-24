using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace _7DaysToDie.Images
{
    public static class ImageHandling
    {
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

                CopyMemory(destination, sourcePtr, (UIntPtr)bytesToCopy);
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
            var encoder = new PngBitmapEncoder();
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
