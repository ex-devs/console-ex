using GleamTech.VideoUltimate;
using System.Diagnostics;

namespace ExtendedConsole
{
    public static class VideoToAscii
    {
        public static List<byte[]> Convert(string filename, out double framerate)
        {
            List<byte[]> frames;
            int i = 0;
            using (var videoFrameReader = new VideoFrameReader(filename))
            {
                int totalFrames = (int)Math.Ceiling(videoFrameReader.FrameRate * videoFrameReader.Duration.TotalSeconds);
                frames = new List<byte[]>(totalFrames);

                ImageToAscii.ScaleImageToConsole(videoFrameReader.Width, videoFrameReader.Height, out int xScale, out int yScale);
                ImageToAscii.GetScaledSize(videoFrameReader.Width, videoFrameReader.Height, xScale, yScale, out int resizedWidth, out int resizedHeight);

                Console.WindowWidth = resizedWidth;
                Console.WindowHeight = resizedHeight;
                Console.BufferWidth = resizedWidth;
                Console.BufferHeight = resizedHeight;

                ProgressBar bar = new(totalFrames, '#', '-', ConsoleColor.Green, ConsoleColor.White);
                var watch = new Stopwatch();
                double medianTimePerFrame = 0;
                double ms = 0;

                foreach(var frame in videoFrameReader)
                {
                    watch.Restart();
                    frames.Add(ImageToAscii.Convert(frame, resizedWidth, resizedHeight, xScale, yScale));
                    i++;
                    ms += watch.ElapsedMilliseconds;
                    medianTimePerFrame = i / ms;
                    Console.Title = $"{i}/{totalFrames} | {watch.ElapsedMilliseconds} ms | eta {(totalFrames-i) * medianTimePerFrame:f0} s";
                    bar.Update(i);
                }
                framerate = videoFrameReader.FrameRate;
            }
            Console.Clear();
            return frames;
        }

        public static void Print(string filename, short fontSize)
        {
            Console.CursorVisible = false;
            ExtendedConsole.SetFont(fontSize);

            List<byte[]> frames = Convert(filename, out double frameRate);
            Console.ReadKey();

            var watch = new Stopwatch();

            double msPerFrame = 1.0 / (frameRate * (1.0 / 1000.0));
            long frequency = Stopwatch.Frequency;

            double ticksPerMs = frequency * (1.0 / 1000.0);
            double ticksPerFrame = ticksPerMs * msPerFrame;

            double mspf;

            int i = 0;
            foreach (var frame in frames)
            {
                watch.Restart();

                ExtendedConsole.WriteViaHandle(frame);

                mspf = watch.ElapsedTicks / ticksPerMs;

                while (watch.ElapsedTicks < ticksPerFrame)
                {

                }

                i++;
                Console.Title = $"{i}/{frames.Count} | FPS: { 1 / ((watch.ElapsedTicks / ticksPerMs) / 1000.0):f2} | mspf: {mspf:f2}";
            }
        }
    }
}
