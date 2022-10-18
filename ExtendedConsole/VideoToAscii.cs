using GleamTech.VideoUltimate;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

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

        public static byte[][] Convert(string filename, out double framerate)
        {
            byte[][] frames;
            int i = 0;
            using (var videoFrameReader = new VideoFrameReader(filename))
            {
                int totalFrames = (int)Math.Ceiling(videoFrameReader.FrameRate * videoFrameReader.Duration.TotalSeconds);
                frames = new byte[totalFrames][];

                ProgressBar bar = new(totalFrames, '#', '-', ConsoleColor.Green, ConsoleColor.White);
                double ms = 0;

                Mutex mutex = new();
                IEnumerator<Bitmap> enumerator = videoFrameReader.GetEnumerator();
                Thread[] threads = new Thread[Environment.ProcessorCount];
                for(int threadID = 0; threadID < threads.Length; threadID++)
                {
                    Thread currentThread = currentThread = new(() =>
                    {
                        var watch = new Stopwatch();
                        Bitmap currentFrame;
                        int frameIndex;

                        while(GetNextFrame(mutex, enumerator, out currentFrame, out frameIndex))
                        {
                            watch.Restart();
                            frames[frameIndex] = ImageToAscii.Convert(currentFrame);
                            //currentFrame.Dispose();
                            ms += watch.ElapsedMilliseconds;
                            Console.Title = $"{frameIndex}/{totalFrames} | {Thread.CurrentThread.ManagedThreadId} {watch.ElapsedMilliseconds} ms | {ms/ frameIndex:f2} ms";
                        }
                    });
                    threads[threadID] = currentThread;
                    currentThread.Start();
                }

                foreach (Thread thread in threads)
                {
                    thread.Join();
                }

                /*
                foreach(var frame in videoFrameReader)
                {
                    watch.Restart();
                    frames.Add(ImageToAscii.Convert(frame));
                    i++;
                    ms += watch.ElapsedMilliseconds;
                    medianTimePerFrame = i / ms;
                    Console.Title = $"{i}/{totalFrames} | {watch.ElapsedMilliseconds} ms | eta {(totalFrames-i) * medianTimePerFrame:f0} s";
                    bar.Update(i);
                }*/
                framerate = videoFrameReader.FrameRate;
            }
            Console.Clear();
            return frames;
        }

        public static void Print(string filename, short fontSize)
        {
            Console.CursorVisible = false;
            ExtendedConsole.SetFont(fontSize);

            byte[][] frames = Convert(filename, out double frameRate);
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
                Console.Title = $"{i}/{frames.Length} | FPS: {1 / ((watch.ElapsedTicks / ticksPerMs) / 1000.0):f2} | mspf: {mspf:f2}";
            }
        }
    }
}
