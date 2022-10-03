using System.Diagnostics;

namespace ExtendedConsole
{
    public class Program
    {
        public static void Main()
        {
            ExtendedConsole ExConsole = new(120, 10, ExtendedConsole.Analytics.None, 6);
            Stopwatch sw = new();
            sw.Start();
            while (true)
            {
                for(int i = 0; i < ExConsole.Height; i++)
                    Console.WriteLine(new string('\u2B1C', ExConsole.Width));
                Console.SetCursorPosition(0, 0);
                sw.Stop();
                ExConsole.Title = Math.Round(1.0 / (sw.ElapsedMilliseconds / 1000.0), 2).ToString() + " FPS";
                sw.Restart();
            } 
        }
    }
}