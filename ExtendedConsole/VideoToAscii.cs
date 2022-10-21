using GleamTech.VideoUltimate;
using System;
using System.Diagnostics;
using System.Drawing;

namespace ExtendedConsole
{
    public static class VideoToAscii
    {
        public static List<byte[]> Convert(string filename, out double framerate)
        {
            List<byte[]> frames;
            int i = 0;
            using (VideoFrameReader videoFrameReader = new VideoFrameReader(filename))
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
                Stopwatch watch = new Stopwatch();
                watch.Start();
                double medianTimePerFrame = 0;
                double ms = 0;
                watch.Start();

                foreach(Bitmap frame in videoFrameReader)
                {
                    frames.Add(ImageToAscii.Convert(frame, resizedWidth, resizedHeight, xScale, yScale));
                    frame.Dispose();
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

            List<byte[]> frames = Convert(filename, out double frameRate);

            Stopwatch watch = new Stopwatch();
            long frequency = Stopwatch.Frequency;

            double msPerFrame = 1.0 / (frameRate * (1.0 / 1000.0));
            double ticksPerMs = frequency * (1.0 / 1000.0);
            double ticksPerFrame = ticksPerMs * msPerFrame;
            double mspf;

            int i = 0;
            foreach (byte[] frame in frames)
            {
                watch.Restart();

                ExtendedConsole.WriteBuffer(frame);

                mspf = watch.ElapsedTicks / ticksPerMs;

                while (watch.ElapsedTicks < ticksPerFrame)
                {

                }

                i++;
                Console.Title = $"{i}/{frames.Count} | FPS: {1 / ((watch.ElapsedTicks / ticksPerMs) / 1000.0):f2} | mspf: {mspf:f2}";
            }
        }

        public static void PrintInParallel(string filename, short fontSize)
        {
            Console.CursorVisible = false;
            ExtendedConsole.SetFont(fontSize);

            Queue<byte[]> frames;
            
            int preRenderAmount = 0;

            using (var videoFrameReader = new VideoFrameReader(filename))
            {
                ImageToAscii.ScaleImageToConsole(videoFrameReader.Width, videoFrameReader.Height, out int xScale, out int yScale);
                ImageToAscii.GetScaledSize(videoFrameReader.Width, videoFrameReader.Height, xScale, yScale, out int resizedWidth, out int resizedHeight);
                Console.WindowWidth = resizedWidth;
                Console.WindowHeight = resizedHeight;
                Console.BufferWidth = resizedWidth;
                Console.BufferHeight = resizedHeight;

                int totalFrames = (int)Math.Ceiling(videoFrameReader.FrameRate * videoFrameReader.Duration.TotalSeconds);
                preRenderAmount = (int)videoFrameReader.FrameRate;
                frames = new(preRenderAmount);
                int renderedFrames = 0;
                int printedFrames = 0;

                double msPerRenderedFrame = 0;
                Stopwatch renderWatch = new();

                Thread renderThread = new(() =>
                {
                    foreach (var frame in videoFrameReader)
                    {
                        renderWatch.Restart();
                        frames.Enqueue(ImageToAscii.Convert(frame, resizedWidth, resizedHeight, xScale, yScale));
                        renderedFrames++;
                        frame.Dispose();
                        msPerRenderedFrame += renderWatch.ElapsedMilliseconds;

                        while(frames.Count >= preRenderAmount)
                        {
                            Thread.CurrentThread.Join((int)(msPerRenderedFrame / renderedFrames));
                        }
                    }
                });
                renderThread.Start();

                Stopwatch watch = new();
                long frequency = Stopwatch.Frequency;

                double msPerFrame = 1.0 / (videoFrameReader.FrameRate * (1.0 / 1000.0));
                double ticksPerMs = frequency * (1.0 / 1000.0);
                double ticksPerFrame = ticksPerMs * msPerFrame;
                double mspf;

                while (printedFrames < totalFrames)
                {
                    if (frames.TryDequeue(out byte[] frame))
                    {
                        watch.Restart();
                        ExtendedConsole.WriteBuffer(frame);
                        mspf = watch.ElapsedTicks / ticksPerMs;

                        while (watch.ElapsedTicks < ticksPerFrame)
                        {

                        }

                        printedFrames++;
                        Console.Title = $"PRF {renderedFrames - printedFrames} | FPS: {1 / (watch.ElapsedTicks / ticksPerMs / 1000.0):f2} | mspf: {mspf:f2} | {printedFrames}/{totalFrames}";
                    }
                }
            }
        }
    }
}
