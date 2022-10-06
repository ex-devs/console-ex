using System.Diagnostics;

namespace ExtendedConsole
{
    public static class DVDScreensaver
    {
        private static readonly string DVD_Raw =
@"
██████  ██    ██ ██████  
██   ██ ██    ██ ██   ██ 
██   ██ ██    ██ ██   ██ 
██   ██  ██  ██  ██   ██ 
██████    ████   ██████";

        private static string[] DVD_Lines = Array.Empty<string>();

        public static void Start(int fpsCap, bool clearScreen = false, bool randomColors = false)
        {
            ExtendedConsole ExConsole = new(101, 20, ExtendedConsole.Analytics.None, 7)
            {
                CursorVisible = false
            };

            Stopwatch sw = new();
            sw.Start();

            // ??????????? i dont know why this works
            double msPerFrame = (1.0/fpsCap) * 1000;

            DVD_Lines = DVD_Raw.Split(Environment.NewLine);

            Random random = new();
            int top = random.Next(0, Console.BufferHeight - DVD_Lines.Length);
            int left = random.Next(0, Console.BufferWidth - DVD_Lines[1].Length);

            int directionX = 0;
            int directionY = 0;;

            double frames = 0;
            double elapsed = 0;

            int cornersHit = 0;

            while (true)
            {
                if (ExConsole.CursorVisible) ExConsole.CursorVisible = false;

                GetDirections(left, top, ref directionX, ref directionY);

                GetNextCursorPos(directionX, directionY, ref left, ref top);

                if (clearScreen) Console.Clear();

                ExConsole.CursorLeft = left;
                ExConsole.CursorTop = top;

                if (frames % 40 == 0) DrawDvD(left, top, randomColors);
                else DrawDvD(left, top);

                if (IsCornerHit(left, top)) cornersHit++;

                // Wait for next Frame
                double ms = sw.ElapsedMilliseconds;
                if (ms < msPerFrame)
                    Thread.Sleep((int)(msPerFrame - sw.ElapsedMilliseconds));

                sw.Stop();
                frames++;
                elapsed += sw.ElapsedMilliseconds;
                ExConsole.Title = $"{(frames / elapsed) * 1000:f2} FPS | CursorPosition({left},{top}) | {ms:f0} ms | Corners: {cornersHit}";
                sw.Restart();
            }
        }

        private static void DrawDvD(int left, int right, bool randomColor)
        {
            if (randomColor)
                Console.ForegroundColor = GetRandomColor();

            DrawDvD(left, right);
        }
        private static void DrawDvD(int left, int right)
        {
            for (int i = 0; i < DVD_Lines.Length; i++)
            {
                Console.Write(DVD_Lines[i]);
                Console.SetCursorPosition(left, right + i);
            }
        }

        private static ConsoleColor GetRandomColor()
        {
            Random random = new();
            return (ConsoleColor)random.Next(1, 14);
        }

        private static void GetNextCursorPos(int directionX, int directionY, ref int left, ref int top)
        {
            // Right
            if (directionX == 0)
                left += 2;
            // Left
            else if (directionX == 1)
                left -= 2;
            // Top
            if (directionY == 0)
                top++;
            // Bottom
            else if (directionY == 1)
                top--;
        }

        private static void GetDirections(int left, int top, ref int directionX, ref int directionY)
        {
            if (left < 2)
                directionX = 0;
            else if (left >= Console.BufferWidth - DVD_Lines[1].Length)
                directionX = 1;
            if (top < 1)
                directionY = 0;
            else if (top >= Console.BufferHeight - DVD_Lines.Length)
                directionY = 1;
        }

        private static bool IsCornerHit(int left, int top)
        {
            if(left == 0 || left == Console.BufferWidth - DVD_Lines[1].Length - 1)
            {
                if (top == 0) return true;
                if (top == Console.BufferHeight - DVD_Lines.Length - 1) return true;
            }

            return false;
        }
    }
}
