using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace ExtendedConsole
{
    public class ExtendedConsole
    {
        #region Properties
        public string Title
        {
            get { return Console.Title; }
            set { Console.Title = value; } 
        } 
        public CONSOLE_FONT_INFO_EX Font
        {
            get
            {
                IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
                if (hnd != INVALID_HANDLE_VALUE)
                {
                    CONSOLE_FONT_INFO_EX info = new();
                    info.cbSize = (uint)Marshal.SizeOf(info);
                    if (GetCurrentConsoleFontEx(hnd, false, ref info))
                    {
                        return info;
                    }
                }
                throw new Exception();
            }
        }
        public int WindowWidth 
        { 
            get { return Console.WindowWidth; } 
        }
        public int WindowHeight
        { 
            get { return Console.WindowHeight; } 
        }
        public int WindowTop
        {
            get { return Console.WindowTop; }
            set { Console.WindowTop = value; }
        }
        public int WindowLeft
        {
            get { return Console.WindowLeft; }
            set { Console.WindowLeft = value; }
        }
        public int LargestWindowWidth
        {
            get { return Console.LargestWindowWidth; }
        }
        public int LargestWindowHeight
        {
            get { return Console.LargestWindowHeight; }
        }
        public int FontSize 
        { 
            get; 
        }
        public int BufferHeight
        {
            get { return Console.BufferHeight; }
            set { Console.BufferHeight = value; }
        }
        public int BufferWidth
        {
            get { return Console.BufferWidth; }
            set { Console.BufferWidth = value; }
        }
        public int CursorLeft
        {
            get { return Console.CursorLeft; }
            set { Console.CursorLeft = value; }
        }
        public int CursorTop
        {
            get { return Console.CursorTop; }
            set { Console.CursorTop = value; }
        }
        public int CursorSize
        {
            get { return Console.CursorSize; }
            set { Console.CursorSize = value; }
        }
        public bool NumberLock
        {
            get { return Console.NumberLock; }
        }
        public bool KeyAvailable
        {
            get { return Console.KeyAvailable; }
        }
        public bool IsOutputRedirected
        {
            get { return Console.IsOutputRedirected; }
        }
        public bool IsInputRedirected
        {
            get { return Console.IsInputRedirected; }
        }
        public bool IsErrorRedirected
        {
            get { return Console.IsErrorRedirected; }
        }
        public bool TreatConrolCAsInput
        {
            get { return Console.TreatControlCAsInput; }
            set { Console.TreatControlCAsInput = value; }
        }
        public bool CursorVisible
        {
            get { return Console.CursorVisible; }
            set { Console.CursorVisible = value; }
        }
        public bool CapsLock
        {
            get { return Console.CapsLock; }
        }
        public Encoding InputEncoding
        {
            get { return Console.InputEncoding; }
            set { Console.InputEncoding = value; }
        }
        public Encoding OutputEncoding
        {
            get { return Console.OutputEncoding; }
            set { Console.OutputEncoding = value; }
        }
        public TextWriter Error
        {
            get { return Console.Error; }       
        }
        public TextWriter Out
        {
            get { return Console.Out; }
        }
        public TextReader In
        {
            get { return Console.In; }
        }
        public ConsoleColor BackroundColor
        {
            get { return Console.BackgroundColor; }
            set { Console.BackgroundColor = value; }
        }
        public ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }
        #endregion

        public enum Analytics
        {
            None,
            Simple,
            Advanced,
        }

        private const short BASE_FONT_SIZE = 24;
        private const string BASE_FONT = "Consolas";

        #region Constructors
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
        #endregion

        private void SetWindowSize()
        {
            double scale = BASE_FONT_SIZE / FontSize;
            int scaledWith = (int)(WindowWidth * scale);
            int scaledHeight = (int)(WindowHeight * scale);
            if(scaledWith > Console.LargestWindowWidth)
                scaledWith = Console.LargestWindowWidth;
            if(scaledHeight > Console.LargestWindowHeight)
                scaledHeight = Console.LargestWindowHeight;
            Console.SetWindowSize(scaledWith, scaledHeight);
            Console.SetBufferSize(scaledWith, scaledHeight);
        }

        public void SetCursorPosition(int left, int top)
            => Console.SetCursorPosition(left, top);

        /// <summary>
        /// credit: https://stackoverflow.com/questions/47014258/c-sharp-modify-console-font-font-size-at-runtime
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="font"></param>
        #region Change Font
        private unsafe void SetFont(short fontSize, string font)
        {
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hnd != INVALID_HANDLE_VALUE)
            {
                CONSOLE_FONT_INFO_EX info = new();
                info.cbSize = (uint)Marshal.SizeOf(info);
                if (GetCurrentConsoleFontEx(hnd, false, ref info))
                {
                    // Set console font to Consola
                    CONSOLE_FONT_INFO_EX newInfo = new();
                    newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                    newInfo.FontFamily = TMPF_TRUETYPE;
                    IntPtr ptr = new(newInfo.FaceName);
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
        private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

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
        public unsafe struct CONSOLE_FONT_INFO_EX
        {
            internal uint cbSize;
            internal uint nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal fixed char FaceName[LF_FACESIZE];
        }
        #endregion
    }
}