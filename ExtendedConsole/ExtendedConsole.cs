using System.Runtime.InteropServices;

namespace ExtendedConsole
{
    public class ExtendedConsole
    {
        public string Title
        { 
            get { return Title; }
            set { Console.Title = value; } 
        } 
        public int Width { get { return Console.WindowWidth; } }
        public int Height { get { return Console.WindowHeight; } }
        public int FontSize { get; }

        private const short BASE_FONT_SIZE = 24;
        private const string BASE_FONT = "Lucida Console";

        public enum Analytics
        {
            None,
            Simple,
            Advanced,
        }

        public ExtendedConsole(int width, int height) : this(width, height, Analytics.None, BASE_FONT_SIZE, BASE_FONT)
        {

        }
        public ExtendedConsole(int width, int height, Analytics analytics) : this(width, height, analytics, BASE_FONT_SIZE, BASE_FONT)
        {

        }
        public ExtendedConsole(int width, int height, Analytics analytics, short fontSize) : this(width, height, analytics, fontSize, BASE_FONT)
        {

        }
        public ExtendedConsole(int width, int height, Analytics analytics, short fontSize, string font)
        {
            FontSize = fontSize;
            SetFont(fontSize, font);
            SetWindowSize();
        }

        private void SetWindowSize()
        {
            double scale = BASE_FONT_SIZE / FontSize;
            int scaledWith = (int)(Width * scale);
            int scaledHeight = (int)(Height * scale);
            Console.SetWindowSize(scaledWith, scaledHeight);
            Console.SetBufferSize(scaledWith, scaledHeight);
        }

        public void SetCursorPosition(int left, int top)
            => Console.SetCursorPosition(left, top);

        private unsafe void SetFont(short fontSize, string font)
        {
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hnd != INVALID_HANDLE_VALUE)
            {
                CONSOLE_FONT_INFO_EX info = new CONSOLE_FONT_INFO_EX();
                info.cbSize = (uint)Marshal.SizeOf(info);
                // First determine whether there's already a TrueType font.
                if (GetCurrentConsoleFontEx(hnd, false, ref info))
                {
                    // Set console font to Lucida Console.
                    CONSOLE_FONT_INFO_EX newInfo = new CONSOLE_FONT_INFO_EX();
                    newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                    newInfo.FontFamily = TMPF_TRUETYPE;
                    IntPtr ptr = new IntPtr(newInfo.FaceName);
                    Marshal.Copy(font.ToCharArray(), 0, ptr, font.Length);
                    // Get some settings from current font.
                    newInfo.dwFontSize = new COORD(fontSize, fontSize);
                    newInfo.FontWeight = info.FontWeight;
                    SetCurrentConsoleFontEx(hnd, false, newInfo);
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool GetCurrentConsoleFontEx(
               IntPtr consoleOutput,
               bool maximumWindow,
               ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCurrentConsoleFontEx(
               IntPtr consoleOutput,
               bool maximumWindow,
               CONSOLE_FONT_INFO_EX consoleCurrentFontEx);

        private const int STD_OUTPUT_HANDLE = -11;
        private const int TMPF_TRUETYPE = 4;
        private const int LF_FACESIZE = 32;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;

            internal COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct CONSOLE_FONT_INFO_EX
        {
            internal uint cbSize;
            internal uint nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal fixed char FaceName[LF_FACESIZE];
        }
    }
}
