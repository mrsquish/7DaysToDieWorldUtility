﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FreeImageAPI;
using NLog;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace _7DaysToDie.Model
{
    public class GreyScalePNG : IDisposable
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();

        private ushort[] _bitMap;
        private Bitmap b16bpp;

        public GreyScalePNG(int size)
        {
            Size = size;
        }

        public int Size { get; }

        public void Dispose()
        {
        }

        public void Create()
        {
            _bitMap = new ushort[Size * Size];
        }

        public void Save(string fileName)
        {
            var rect = new Rectangle(0, 0, Size, Size);
            b16bpp = new Bitmap(Size, Size, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);
            var bitmapData = b16bpp.LockBits(rect, ImageLockMode.WriteOnly, b16bpp.PixelFormat);

            // Copy the randomized bits to the bitmap pointer.
            var ptr = bitmapData.Scan0;
            Copy(_bitMap, ptr, 0, _bitMap.Length);

            // Unlock the bitmap, we're all done.
            b16bpp.UnlockBits(bitmapData);
            SaveBmp(b16bpp, fileName);
        }

        public void SetPixel(int x, int y, ushort shade)
        {
            var i = ((y * Size) + x); // 16bpp
            _bitMap[i] = shade;         // GRAY
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

            var encoder = new TiffBitmapEncoder {Compression = TiffCompressOption.Zip};

            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);

            stream.Close();
        }

        private static PixelFormat ConvertBmpPixelFormat(System.Drawing.Imaging.PixelFormat pixelformat)
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