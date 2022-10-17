using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace ExtendedConsole
{
    public static class ImageToAscii
    {
        public static byte[] Convert(string filename)
        {
            return Convert(new Bitmap(filename));
        }
        public static byte[] Convert(Bitmap? bitmap)
        {
            if (bitmap == null) return null;
            return GetAverageColors(bitmap);
        }

        const string chars = " .'`^\",:;Il!i><~+_-?][}{1)(|\\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$";
        static readonly int maxIndex = chars.Length - 1;

        private static char GetChar(float brightness)
        {
            return chars[(int)Math.Ceiling(brightness * maxIndex)];
        }


        public static byte[] GetAverageColors(Bitmap bitmap)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            int xStep = (int)Math.Ceiling((double)width / Console.LargestWindowWidth);
            int yStep = (int)Math.Ceiling((double)height / Console.LargestWindowHeight);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int resizedWidth = (int)Math.Ceiling((double)width / xStep);
            int resizedHeight = (int)Math.Ceiling((double)height / yStep);

            Console.WindowWidth = resizedWidth;
            Console.WindowHeight = resizedHeight;
            Console.BufferWidth = resizedWidth;
            Console.BufferHeight = resizedHeight;

            int elementsPerGroup = xStep * yStep;
            byte[] avgColors = new byte[resizedHeight * resizedWidth];
            float r = 0;
            float g = 0;
            float b = 0;
            int avgColorsIndex = 0;

            unsafe
            {
                byte* basePtr = (byte*)data.Scan0.ToPointer();

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
                                r += basePtr[3 * (width * i + j)];
                                g += basePtr[3 * (width * i + j) + 1];
                                b += basePtr[3 * (width * i + j) + 2];
                            }
                        }

                        avgColors[avgColorsIndex] = (byte)GetChar(Color.GetBrightness((byte)(r / elementsPerGroup), (byte)(g / elementsPerGroup), (byte)(b / elementsPerGroup)));
                    }
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
