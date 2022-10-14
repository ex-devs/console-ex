using System.Drawing;

namespace ExtendedConsole
{
    public static class ImageToAscii
    {
        public static string[] Convert(Bitmap bitmap)
        {
            ExtendedConsole.SetFont(1);

            var colors = GetAverageColors(bitmap);

            List<string> result = new List<string>();
            for (int i = 0; i < colors.Count; i++)
            {
                string row = "";
                for (int j = 0; j < colors[i].Count; j++)
                {
                    row += GetChar(colors[i][j]);
                }
                result.Add(row);
            }

            return result.ToArray();
        }
        public static string[] Convert(string source)
        {
            Bitmap bitmap = new(source);
            if (bitmap == null) throw new Exception();

            return Convert(bitmap);
        }

        private static string GetChar(Color color)
        {
            const string chars = " .'`^\",:;Il!i><~+_-?][}{1)(|\\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$";
            var brightness = color.GetBrightness();

            float step = 1 / ((float)chars.Length-1);

            for (int i = 0; i < chars.Length; i++)
                if (brightness <= i * step && brightness >= (i - 1) * step) return new string(chars[i], 1);

            return " ";
        }

        private static List<List<Color>> GetAverageColors(Bitmap bitmap)
        {
            int xStep = (int)Math.Ceiling((double)bitmap.Width / Console.BufferWidth);
            int yStep = (int)Math.Ceiling((double)bitmap.Height / Console.BufferHeight);

            List<List<Color>> averageColors = new(bitmap.Width / xStep * bitmap.Height / yStep);
           
            for (int i = 0; i < bitmap.Height - yStep; i += yStep)
            {
                List<Color> averageColorsRow = new();
                for (int j = 0; j < bitmap.Width - xStep; j += xStep)
                {
                    List<Color> list = new(yStep * xStep);
                    for (int k = i; k < i + yStep; k++)
                    {
                        for (int l = j; l < j + xStep; l++)
                        {
                            list.Add(bitmap.GetPixel(l, k));
                        }
                    }

                    int avgA = 0;
                    int avgR = 0;
                    int avgG = 0;
                    int avgB = 0;
                    foreach (Color c in list)
                    {
                        avgA += c.A;
                        avgR += c.R;
                        avgG += c.G;
                        avgB += c.B;
                    }
                    averageColorsRow.Add(Color.FromArgb(avgA / list.Count, avgR / list.Count, avgG / list.Count, avgB / list.Count));
                }
                averageColors.Add(averageColorsRow);
            }
            return averageColors;
        }

        public static void Print(string[] asciiImage)
        {
            foreach (string line in asciiImage)
            {
                Console.WriteLine(line);
            }
        }
    }
}
