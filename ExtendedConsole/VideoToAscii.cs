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
            using (VideoFrameReader videoFrameReader = new(filename))
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
                Stopwatch watch = new();
                watch.Start();
                double medianTimePerFrame = 0;
                double ms = 0;

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

            Stopwatch watch = new();
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

            using VideoFrameReader videoFrameReader = new(filename);

            ImageToAscii.ScaleImageToConsole(videoFrameReader.Width, videoFrameReader.Height, out int xScale, out int yScale);
            ImageToAscii.GetScaledSize(videoFrameReader.Width, videoFrameReader.Height, xScale, yScale, out int resizedWidth, out int resizedHeight);
            Console.WindowWidth = resizedWidth;
            Console.WindowHeight = resizedHeight;
            Console.BufferWidth = resizedWidth;
            Console.BufferHeight = resizedHeight;

            int totalFrames = (int)Math.Floor(videoFrameReader.FrameRate * videoFrameReader.Duration.TotalSeconds);

            preRenderAmount = (int)Math.Ceiling(videoFrameReader.FrameRate);
            int renderedFrames = 0;
            int printedFrames = 0;
            double msPerRenderedFrame = 0;

            frames = new(preRenderAmount);
            Stopwatch renderWatch = new();

            Thread renderThread = new(() =>
            {
                foreach (Bitmap frame in videoFrameReader)
                {
                    renderWatch.Restart();
                    frames.Enqueue(ImageToAscii.Convert(frame, resizedWidth, resizedHeight, xScale, yScale));
                    renderedFrames++;
                    frame.Dispose();
                    msPerRenderedFrame += renderWatch.ElapsedMilliseconds;

                    while (frames.Count >= preRenderAmount)
                    {
                        Thread.CurrentThread.Join((int)(msPerRenderedFrame / renderedFrames));
                    }
                }
            });
            renderThread.Start();

            Stopwatch watch = new();
            
            long frequency = Stopwatch.Frequency;

            double msPerFrame = 1000.0 / videoFrameReader.FrameRate;
            double ticksPerMs = frequency / 1000.0;
            double ticksPerFrame = ticksPerMs * msPerFrame;
            double mspf;

            watch.Start();

            double fps;
            while (printedFrames < totalFrames)
            {
                if (frames.TryDequeue(out byte[]? frame))
                {
                    ExtendedConsole.WriteBuffer(frame);
                    mspf = watch.ElapsedTicks / ticksPerMs;

                    while (watch.ElapsedTicks < ticksPerFrame)
                    {
                        
                    }

                    fps = (double)frequency / watch.ElapsedTicks;
                    printedFrames++;
                    watch.Restart();
                    Console.Title = $"PRF {renderedFrames - printedFrames} | FPS: {fps:f2} | mspf: {mspf:f2} | {printedFrames}/{totalFrames}";
                }
            }
        }
    }
}
