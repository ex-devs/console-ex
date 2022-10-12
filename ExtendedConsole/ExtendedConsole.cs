using System.Runtime.InteropServices;

namespace ExtendedConsole
{
    public static class ExtendedConsole
    {
        #region Properties
        public static CONSOLE_FONT_INFO_EX Font
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
        public unsafe static short FontSize
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
                        return info.dwFontSize.Y;
                    }
                }
                throw new Exception();
            }
            set
            {
                SetFont(value);
            }
        }
        #endregion

        // Todo: code duplication
        // credit: https://stackoverflow.com/questions/47014258/c-sharp-modify-console-font-font-size-at-runtime
        #region Change Font
        public unsafe static void SetFont(short fontSize, string font)
        {
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hnd != INVALID_HANDLE_VALUE)
            {
                CONSOLE_FONT_INFO_EX info = new();
                info.cbSize = (uint)Marshal.SizeOf(info);
                if (GetCurrentConsoleFontEx(hnd, false, ref info))
                {
                    // Set console font
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
        public unsafe static void SetFont(short fontSize)
        {
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hnd != INVALID_HANDLE_VALUE)
            {
                CONSOLE_FONT_INFO_EX info = new();
                info.cbSize = (uint)Marshal.SizeOf(info);
                if (GetCurrentConsoleFontEx(hnd, false, ref info))
                {
                    // Set console font
                    CONSOLE_FONT_INFO_EX newInfo = Font;
                    newInfo.dwFontSize = new COORD(fontSize, fontSize);
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