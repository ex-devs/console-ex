using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace ExtendedConsole
{
    public static class ImageToAscii
    {
        public static ExtendedConsole.CHAR_INFO[] Convert(string filename, out short rows, out short cols)
        {
            return Convert(new Bitmap(filename), out rows, out cols);
        }
        public static ExtendedConsole.CHAR_INFO[] Convert(Bitmap? bitmap, out short rows, out short cols)
        {
            rows = 0;
            cols = 0;
            if (bitmap == null) return null;
            return GetAverageColors(bitmap, out rows, out cols);
        }

        private static char GetChar(float brightness)
        {
            const string chars = " .'`^\",:;Il!i><~+_-?][}{1)(|\\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$";

            double step = 1.0 / (chars.Length - 1);

            for (int i = 0; i < chars.Length; i++)
                if (brightness <= i * step && brightness >= (i - 1) * step) return chars[i];

            return chars[0];
        }


        public static ExtendedConsole.CHAR_INFO[] GetAverageColors(Bitmap bitmap, out short rows, out short cols)
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

            rows = (short)resizedHeight;
            cols = (short)resizedWidth;

            int elementsPerGroup = xStep * yStep;
            ExtendedConsole.CHAR_INFO[] avgColors = new ExtendedConsole.CHAR_INFO[resizedHeight * resizedWidth];
            float r = 0;
            float g = 0;
            float b = 0;
            int avgColorsIndex = 0;

            // @ImZorg 😎😎😎😎😎😎😎
            for (int row = 0; row < height; row += yStep) 
            {
                for (int col = 0; col < width; col += xStep) 
                {
                    r = 0;
                    g = 0;
                    b = 0;
                    avgColorsIndex = (row / yStep) * resizedWidth + (col / xStep);

                    for (int i = row; i < height && i < row + yStep; i++)
                    {
                        for (int j = col; j < width && j < col + xStep; j++)
                        {
                            r += bitmapData[3 * (width * i + j)];
                            g += bitmapData[3 * (width * i + j) + 1];
                            b += bitmapData[3 * (width * i + j) + 2];
                        }
                    }

                    avgColors[avgColorsIndex].UnicodeChar = GetChar(Color.GetBrightness((byte)(r / elementsPerGroup), (byte)(g / elementsPerGroup), (byte)(b / elementsPerGroup)));
                    avgColors[avgColorsIndex].Attributes = 0x0008 | 0x0004 | 0x0002 | 0x0001;
                }
            }
            bitmap.UnlockBits(data);
            return avgColors;
        }

        public static void Print(string asciiImage)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(asciiImage);
        }
    }
}
