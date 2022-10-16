using GleamTech.VideoUltimate;
using System.Diagnostics;

namespace ExtendedConsole
{
    public static class VideoToAscii
    {
        public static List<byte[]> Convert(string filename, out double framerate)
        {
            List<byte[]> frames = new();
            int i = 0;
            using (var videoFrameReader = new VideoFrameReader(filename))
            {
                int totalFrames = (int)(videoFrameReader.FrameRate * videoFrameReader.Duration.TotalSeconds);
                ProgressBar bar = new(totalFrames, '#', '-', ConsoleColor.Green, ConsoleColor.White);
                var watch = new Stopwatch();
                foreach(var frame in videoFrameReader)
                {
                    watch.Restart();
                    frames.Add(ImageToAscii.Convert(frame));
                    i++; 
                    Console.Title = $"{i}/{totalFrames} | {watch.ElapsedMilliseconds} ms";
                    bar.Update(i);
                }
                framerate = videoFrameReader.FrameRate;
            }
            Console.Clear();
            return frames;
        }

        public static void Print(List<byte[]> frames, double frameRate)
        {
            Console.ReadKey();
            //byte[] buffer = System.Text.Encoding.ASCII.GetBytes(new string('F', Console.BufferHeight * Console.BufferWidth));
            //byte[] buffer = System.Text.Encoding.ASCII.GetBytes(new string('F', 4));
            var watch = new Stopwatch();

            double msPerFrame = 1.0 / (frameRate * (1.0 / 1000.0));

            int i = 0;
            foreach (var frame in frames)
            {
                watch.Restart();

                //Console.SetCursorPosition(0, 0);
                //ExtendedConsole.WriteBuffer(frame, rows, cols);
                //Console.WriteLine(frame);
                //ExtendedConsole.WriteViaHandle(buffer);

                //Console.SetCursorPosition(0, 0);
                ExtendedConsole.WriteViaHandle(frame);

                while (watch.ElapsedMilliseconds < msPerFrame)
                {

                }

                i++;
                Console.Title = $"{i}/{frames.Count} | FPS: { 1 / (watch.ElapsedMilliseconds / 1000.0):f2}";
            }

            
        }
    }
}
