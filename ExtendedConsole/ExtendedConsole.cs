﻿using System.Runtime.InteropServices;

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
        public struct COORD
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

        #region WriteConsoleOutput
        [StructLayout(LayoutKind.Explicit)]
        public struct CHAR_INFO
        {
            [FieldOffset(0)]
            internal char UnicodeChar;
            [FieldOffset(0)]
            internal char AsciiChar;
            [FieldOffset(2)] //2 bytes seems to work properly
            internal UInt16 Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        [DllImport("kernel32.dll")]
        static extern bool FlushFileBuffers(IntPtr hFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteConsoleOutput(
        IntPtr hConsoleOutput,
        CHAR_INFO[] lpBuffer,
        COORD dwBufferSize,
        COORD dwBufferCoord,
        ref SMALL_RECT lpWriteRegion
        );

        static IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
        static SMALL_RECT lpBuffer = new SMALL_RECT() { Left = 0, Top = 0, Right = (short)(Console.BufferWidth - 1), Bottom = (short)(Console.BufferHeight - 1) };

        public static void WriteBuffer(CHAR_INFO[] buffer, short rows, short columns)
        {
            WriteConsoleOutput(hnd, buffer, new COORD() { X = columns, Y = rows }, new COORD() { X = 0, Y = 0 }, ref lpBuffer);

            //FlushFileBuffers(hnd);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        internal static extern int WriteFile(IntPtr handle, ref byte bytes, int numBytesToWrite, out int numBytesWritten, IntPtr mustBeZero);


        public static void WriteViaHandle(byte[] buffer)
        {
            int written;

            WriteFile(hnd, ref buffer[0], buffer.Length, out written, IntPtr.Zero);
        }
        #endregion
    }
}