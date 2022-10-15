﻿using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace ExtendedConsole
{
    public static class ImageToAscii
    {
        public static string Convert(string filename)
        {
            return Convert(new Bitmap(filename));
        }
        public static string Convert(Bitmap? bitmap)
        {
            if (bitmap == null) return string.Empty;
            return GetAverageColors(bitmap);
        }

        private static string GetChar(float brightness)
        {
            const string chars = " .'`^\",:;Il!i><~+_-?][}{1)(|\\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$";

            double step = 1.0 / (chars.Length - 1);

            for (int i = 0; i < chars.Length; i++)
                if (brightness <= i * step && brightness >= (i - 1) * step) return new string(chars[i], 1);

            return new string(chars[0], 1);
        }


        public static string GetAverageColors(Bitmap bitmap)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            int xStep = (int)Math.Ceiling((double)width / Console.BufferWidth);
            int yStep = (int)Math.Ceiling((double)height / Console.BufferHeight);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            IntPtr ptr = data.Scan0;
            int bytes = Math.Abs(data.Stride) * data.Height;
            byte[] bitmapData = new byte[bytes];
            Marshal.Copy(ptr, bitmapData, 0, bytes);

            int resizedWidth = (int)Math.Ceiling((double)width / xStep);
            int resizedHeight = (int)Math.Ceiling((double)height / yStep);

            int elementsPerGroup = xStep * yStep;
            StringBuilder frame = new(resizedHeight * resizedWidth);
            StringBuilder line = new(resizedWidth);
            float r = 0;
            float g = 0;
            float b = 0;

            // @ImZorg 😎😎😎😎😎😎😎
            for (int row = 0; row < height; row += yStep) 
            {
                line.Clear();
                for (int col = 0; col < width; col += xStep) 
                {
                    r = 0;
                    g = 0;
                    b = 0;

                    for (int i = row; i < height && i < row + yStep; i++)
                    {
                        for (int j = col; j < width && j < col + xStep; j++)
                        {
                            r += bitmapData[3 * (width * i + j)];
                            g += bitmapData[3 * (width * i + j) + 1];
                            b += bitmapData[3 * (width * i + j) + 2];
                        }
                    }

                    line.Append(GetChar(Color.GetBrightness((byte)(r / elementsPerGroup), (byte)(g / elementsPerGroup), (byte)(b / elementsPerGroup))));
                }
                frame.AppendLine(line.ToString());
            }
            bitmap.UnlockBits(data);
            return frame.ToString();
        }

        public static void Print(string asciiImage)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(asciiImage);
        }
    }
}
