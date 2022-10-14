namespace ExtendedConsole
{
    public class Program
    {
        public static void Main()
        {
            //ImageToAscii.Print(ImageToAscii.Convert(""));

            var frames = VideoToAscii.Convert("VID-20220318-WA0001.mp4");

            foreach (var frame in frames)
            {
                Thread.Sleep(16);
                Console.SetCursorPosition(0, 0);
                foreach (var line in frame)
                {
                    Console.WriteLine(line);
                }
            }
        }
    }
}