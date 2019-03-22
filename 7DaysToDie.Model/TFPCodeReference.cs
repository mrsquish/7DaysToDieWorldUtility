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
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="_fac"></param>
        /// <param name="_clampHeight"></param>
        /// <returns></returns>
        public static ushort[] LoadHeightMapRAW(
            string _filePath,
            int w,
            int h,
            float _fac = 1f,
            float _clampHeight = 1f)
        {
            using (BufferedStream bufferedStream = new BufferedStream((Stream)File.OpenRead(_filePath)))
            {
                _clampHeight *= 256f;
                byte[] buffer = new byte[8192];
                ushort[] numArray = new ushort[w * h];
                int num1 = 0;
                int num2 = 0;
                int num3 = 0;
                while ((long)num3 < bufferedStream.Length)
                {
                    int num4 = bufferedStream.Read(buffer, 0, buffer.Length);
                    num3 += num4;
                    int index = 0;
                    int num5 = num1 + num2 * w;
                    for (; index < num4; index += 2)
                    {
                        byte num6 = buffer[index];
                        ushort num7 = (ushort)((uint)buffer[index + 1] * 256U + (uint)num6);
                        if ((double)_clampHeight > 0.0 && (double)num7 > (double)_clampHeight)
                            num7 = (ushort)_clampHeight;
                        numArray[num5++] = num7;
                        ++num1;
                        if (num1 >= w)
                        {
                            num1 = 0;
                            ++num2;
                            num5 = num1 + num2 * w;
                        }
                    }
                }
                return numArray;
            }
        }
    }
}
