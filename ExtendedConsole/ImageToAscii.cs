using System.Drawing;
using System.Drawing.Imaging;

namespace ExtendedConsole
{
    public static class ImageToAscii
    {
        const string chars = " .'`^\",:;Il!i><~+_-?][}{1)(|\\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$";
        static readonly int maxIndex = chars.Length - 1;

        private static char GetChar(float brightness)
        {
            return chars[(int)Math.Ceiling(brightness * maxIndex)];
        }

        public static void ScaleImageToConsole(int width, int height, out int xScale, out int yScale)
        {
            xScale = (int)Math.Ceiling((double)width / Console.LargestWindowWidth);
            yScale = (int)Math.Ceiling((double)height / Console.LargestWindowHeight);
        }

        public static void GetScaledSize(int width, int height, int xScale, int yScale, out int resizedWidth, out int resizedHeight)
        {
            resizedWidth = (int)Math.Ceiling((double) width / xScale);
            resizedHeight = (int)Math.Ceiling((double) height / yScale);
        }

        public static byte[] Convert(string filename)
        {
            Bitmap bitmap = new Bitmap(filename);
            ScaleImageToConsole(bitmap.Width, bitmap.Height, out int xScale, out int yScale);
            GetScaledSize(bitmap.Width, bitmap.Height, xScale, yScale, out int resizedWidth, out int resizedHeight);

            return Convert(bitmap, resizedWidth, resizedHeight, xScale, yScale);
        }

        public static byte[] Convert(Bitmap bitmap, int resizedWidth, int resizedHeight, int xScale, int yScale)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int elementsPerGroup = xScale * yScale;
            byte[] avgColors = new byte[resizedHeight * resizedWidth];
            float r = 0;
            float g = 0;
            float b = 0;
            int avgColorsIndex = 0;

            unsafe
            {
                byte* basePtr = (byte*)data.Scan0.ToPointer();

                // @ImZorg 😎😎😎😎😎😎😎
                for (int row = 0; row < height; row += yScale)
                {
                    for (int col = 0; col < width; col += xScale)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                        avgColorsIndex = (row / yScale) * resizedWidth + (col / xScale);

                        for (int i = row; i < height && i < row + yScale; i++)
                        {
                            for (int j = col; j < width && j < col + xScale; j++)
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
