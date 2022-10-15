namespace ExtendedConsole
{
    public class Program
    {
        public static void Main()
        {
            Console.CursorVisible = false;
            ExtendedConsole.SetFont(1);
            VideoToAscii.Print(VideoToAscii.Convert("", out double frameRate, out short rows, out short cols), rows, cols, frameRate);
        }
    }
}