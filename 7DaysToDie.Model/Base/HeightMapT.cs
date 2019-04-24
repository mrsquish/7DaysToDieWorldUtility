using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NLog;
using _7DaysToDie.Images;

namespace _7DaysToDie.Base
{
    public class HeightMap<T>
    {
        protected static ILogger _logger = LogManager.GetCurrentClassLogger();

        public T[] Map;

        public HeightMap(int size)
        {
            Size = size;
            Create();
        }

        public int Size { get; }

        public T this[int x, int z]
        {
            get => Map[z * Size + x];
            set => Map[z * Size + x] = value;
        }


        public void Initialise(T initialHeight)
        {
            for (var i = 0; i < Map.Length - 1; i++) Map[i] = initialHeight;
        }

        public void Create()
        {
            Map = new T[Size * Size];
        }

        public virtual void Save(string fileName, Func<T, ushort> convertToUnsignedShort)
        {
            var bitMap = SaveRaw(Path.ChangeExtension(fileName, "raw"), convertToUnsignedShort);
            SavePng(Path.ChangeExtension(fileName, "png"), bitMap);
        }

        private void SavePng(string fileName, ushort[] bitMap)
        {
            var rect = new Rectangle(0, 0, Size, Size);
            var b16Bpp = new Bitmap(Size, Size, PixelFormat.Format16bppGrayScale);
            var bitmapData = b16Bpp.LockBits(rect, ImageLockMode.WriteOnly, b16Bpp.PixelFormat);

            // Copy the randomized bits to the bitmap pointer.
            var ptr = bitmapData.Scan0;
            ImageHandling.Copy(bitMap, ptr, 0, bitMap.Length);

            // Unlock the bitmap, we're all done.
            b16Bpp.UnlockBits(bitmapData);
            ImageHandling.SaveBmp(b16Bpp, fileName);
        }

        private ushort[] SaveRaw(string filename, Func<T, ushort> convertToUnsignedShort)
        {
            var bitMap = new ushort[Size * Size];
            using (var writer = new BinaryWriter(File.Create(filename)))
            {
                for (var row = 0; row < Size; row++)
                for (var column = 0; column < Size; column++)
                {
                    //BinaryWriter will write the value in little Endian. 
                    var output = convertToUnsignedShort(Map[row * Size + column]);
                    bitMap[row * Size + column] = output;
                    writer.Write(output);
                }
            }

            return bitMap;
        }

        public void SetPixel(int x, int y, T shade)
        {
            var i = y * Size + x;
            Map[i] = shade;
        }
    }
}