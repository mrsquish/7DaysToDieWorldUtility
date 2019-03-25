using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model
{
    public class TFPCodeReference
    {
        /// <summary>
        /// This is the function that loads the dtm.raw file.
        /// </summary>
        /// <param name="_filePath"></param>
        /// <param name="width"></param>
        /// <param name="h"></param>
        /// <param name="_fac"></param>
        /// <param name="_clampHeight"></param>
        /// <returns></returns>
        public static ushort[] LoadHeightMapRAW(
            string _filePath,
            int width,
            int height,
            float _fac = 1f,
            float _clampHeight = 1f)
        {
            using (BufferedStream bufferedStream = new BufferedStream((Stream)File.OpenRead(_filePath)))
            {
                _clampHeight *= 256f;

                byte[] buffer = new byte[8192];

                ushort[] numArray = new ushort[width * height];

                int num1 = 0;
                int num2 = 0;
                int totalNumberOfBytesRead = 0;

                while ((long)totalNumberOfBytesRead < bufferedStream.Length)
                {
                    var numberofBytesRead = bufferedStream.Read(buffer, 0, buffer.Length);
                    totalNumberOfBytesRead += numberofBytesRead;

                    int index = 0;
                    int bitMapIndex = num1 + num2 * width;

                    for (; index < numberofBytesRead; index += 2)
                    {
                        byte num6 = buffer[index];

                        ushort mapHeight = (ushort)((uint)buffer[index + 1] * 256U + (uint)num6);

                        if ((double)_clampHeight > 0.0 && (double)mapHeight > (double)_clampHeight)
                            mapHeight = (ushort)_clampHeight;

                        numArray[bitMapIndex++] = mapHeight;

                        ++num1;
                        if (num1 >= width)
                        {
                            num1 = 0;
                            ++num2;
                            bitMapIndex = num1 + num2 * width;
                        }
                    }
                }
                return numArray;
            }
        }
    }
}
