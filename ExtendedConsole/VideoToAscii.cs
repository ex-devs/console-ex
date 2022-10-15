using GleamTech.VideoUltimate;
using System.Diagnostics;

namespace ExtendedConsole
{
    public static class VideoToAscii
    {
        public static List<ExtendedConsole.CHAR_INFO[]> Convert(string filename, out double framerate, out short rows, out short cols)
        {
            List<ExtendedConsole.CHAR_INFO[]> frames = new();
            rows = 0;
            cols = 0;
            int i = 0;
            using (var videoFrameReader = new VideoFrameReader(filename))
            {
                int totalFrames = (int)(videoFrameReader.FrameRate * videoFrameReader.Duration.TotalSeconds);
                ProgressBar bar = new(totalFrames, '#', '-', ConsoleColor.Green, ConsoleColor.White);
                var watch = new Stopwatch();
                foreach(var frame in videoFrameReader)
                {
                    watch.Restart();
                    frames.Add(ImageToAscii.Convert(frame, out rows, out cols));
                    i++; 
                    Console.Title = $"{i}/{totalFrames} | {watch.ElapsedMilliseconds} ms";
                    bar.Update(i);
                }
                framerate = videoFrameReader.FrameRate;
            }
            Console.Clear();
            return frames;
        }

        public static void Print(List<ExtendedConsole.CHAR_INFO[]> frames, short rows, short cols, double frameRate)
        {
            var watch = new Stopwatch();
            long ms = 0;

            double msPerFrame = 1 / (frameRate * 1 / 1000);

            int i = 0;
            foreach (var frame in frames)
            {
                watch.Restart();

                //Console.SetCursorPosition(0, 0);
                ExtendedConsole.WriteBuffer(frame, rows, cols);
                //Console.WriteLine(frame);

                while (watch.ElapsedMilliseconds < msPerFrame)
                {

                }

                ms += watch.ElapsedMilliseconds;
                i++;
                Console.Title = $"{i}/{frames.Count} | FPS: {i / (ms / 1000.0):f2}";
            }
        }
    }
}
