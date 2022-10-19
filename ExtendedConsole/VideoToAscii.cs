using GleamTech.VideoUltimate;
using System.Diagnostics;
using System.Drawing;

namespace ExtendedConsole
{
    public static class VideoToAscii
    {
        private static int Frame = 0;

        private static bool GetNextFrame(Mutex mutex, IEnumerator<Bitmap> enumerator, out Bitmap bitmap, out int frame)
        {
            mutex.WaitOne();
            bool status = enumerator.MoveNext();
            bitmap = enumerator.Current;
            frame = Frame;
            Frame++;
            mutex.ReleaseMutex();
            return status;
        }

        private static bool GetNextFrames(Mutex mutex, IEnumerator<Bitmap> enumerator, out Bitmap[] bitmaps, out int startIndex, out int length)
        {
            mutex.WaitOne();

            length = 0;
            startIndex = Frame;
            Bitmap[] frames = new Bitmap[20];
            while (enumerator.MoveNext())
            {
                frames[length] = enumerator.Current;
                length++;
                if (length == 20) break;
            }
            Frame += length;
            bitmaps = frames;
            mutex.ReleaseMutex();
            return length > 0;
        }

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
                bar.Update(0);
                double ms = 0;
                double medianTimePerFrame = 0;
                var watch = new Stopwatch();
                watch.Start();

                foreach(var frame in videoFrameReader)
                {
                    frames.Add(ImageToAscii.Convert(frame, resizedWidth, resizedHeight, xScale, yScale));
                    i++;
                    ms += watch.ElapsedMilliseconds;
                    medianTimePerFrame = ms / i;
                    Console.Title = $"{i}/{totalFrames} | {watch.ElapsedMilliseconds} ms | eta {(totalFrames - i) * (medianTimePerFrame / 1000.0):f0} s";
                    bar.Update(i);
                    watch.Restart();
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

            var frames = Convert(filename, out double frameRate);
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
                Console.Title = $"{i}/{frames.Count} | FPS: {1 / ((watch.ElapsedTicks / ticksPerMs) / 1000.0):f2} | mspf: {mspf:f2}";
            }
        }
    
        public static void PrintAsync(string filename, short fontSize)
        {
            Console.CursorVisible = false;
            ExtendedConsole.SetFont(fontSize);

            Stack<byte[]> frames;
            
            int preRenderAmount = 0;

            using (var videoFrameReader = new VideoFrameReader(filename))
            {
                preRenderAmount = (int)videoFrameReader.FrameRate;
                ImageToAscii.ScaleImageToConsole(videoFrameReader.Width, videoFrameReader.Height, out int xScale, out int yScale);
                ImageToAscii.GetScaledSize(videoFrameReader.Width, videoFrameReader.Height, xScale, yScale, out int resizedWidth, out int resizedHeight);

                Console.WindowWidth = resizedWidth;
                Console.WindowHeight = resizedHeight;
                Console.BufferWidth = resizedWidth;
                Console.BufferHeight = resizedHeight;

                int totalFrames = (int)Math.Ceiling(videoFrameReader.FrameRate * videoFrameReader.Duration.TotalSeconds);
                frames = new(preRenderAmount);
                int framesRendered = 0;
                Thread renderThread = new(() =>
                {
                    foreach(var frame in videoFrameReader)
                    {
                        frames.Push(ImageToAscii.Convert(frame,resizedWidth, resizedHeight, xScale, yScale));
                        framesRendered++;
                        frame.Dispose();
                        while (frames.Count == preRenderAmount)
                        {

                        }
                    }
                });

                renderThread.Start();

                var watch = new Stopwatch();

                double msPerFrame = 1.0 / (videoFrameReader.FrameRate * (1.0 / 1000.0));
                long frequency = Stopwatch.Frequency;
                double ticksPerMs = frequency * (1.0 / 1000.0);
                double ticksPerFrame = ticksPerMs * msPerFrame;
                double mspf;

                int printedFrames = 0;

                while (printedFrames < totalFrames)
                {
                    if (frames.TryPop(out byte[] frame))
                    {
                        watch.Restart();
                        ExtendedConsole.WriteViaHandle(frame);
                        mspf = watch.ElapsedTicks / ticksPerMs;

                        while (watch.ElapsedTicks < ticksPerFrame)
                        {

                        }

                        printedFrames++;
                        Console.Title = $"{framesRendered - printedFrames} | FPS: {1 / ((watch.ElapsedTicks / ticksPerMs) / 1000.0):f2} | mspf: {mspf:f2}";
                    }
                }
                renderThread.Join();
            }
        }
    }
}
