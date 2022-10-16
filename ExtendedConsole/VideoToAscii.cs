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
            var watch = new Stopwatch();

            double msPerFrame = 1.0 / (frameRate * (1.0 / 1000.0));
            long frequency = Stopwatch.Frequency;

            double ticksPerMs = frequency * (1.0 / 1000.0);
            double ticksPerFrame = ticksPerMs * msPerFrame;

            int i = 0;
            foreach (var frame in frames)
            {
                watch.Restart();

                ExtendedConsole.WriteViaHandle(frame);

                while (watch.ElapsedTicks < ticksPerFrame)
                {

                }

                i++;
                Console.Title = $"{i}/{frames.Count} | FPS: { 1 / ((watch.ElapsedTicks / ticksPerMs)/ 1000.0):f2}";
            }

            
        }
    }
}
