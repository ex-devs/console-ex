﻿using GleamTech.VideoUltimate;
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

        public static byte[][] Convert(string filename, out double framerate)
        {
            byte[][] frames;
            int i = 0;
            using (var videoFrameReader = new VideoFrameReader(filename))
            {
                int totalFrames = (int)Math.Ceiling(videoFrameReader.FrameRate * videoFrameReader.Duration.TotalSeconds);
                frames = new byte[totalFrames][];

                ProgressBar bar = new(totalFrames, '#', '-', ConsoleColor.Green, ConsoleColor.White);
                bar.Update(0);
                double ms = 0;

                Mutex mutex = new();
                IEnumerator<Bitmap> enumerator = videoFrameReader.GetEnumerator();
                Thread[] threads = new Thread[Environment.ProcessorCount];
                for(int threadID = 0; threadID < threads.Length; threadID++)
                {
                    Thread currentThread = currentThread = new(() =>
                    {
                        var watch = new Stopwatch();

                        while (GetNextFrames(mutex, enumerator, out Bitmap[] frameRange, out int startIndex, out int length))
                        {
                            for (int frameIndex = 0; frameIndex < length; frameIndex++)
                            {
                                watch.Restart();
                                frames[startIndex] = ImageToAscii.Convert(frameRange[frameIndex]);
                                startIndex++;
                                ms += watch.ElapsedMilliseconds;
                                Console.Title = $"{Frame}/{totalFrames} | {watch.ElapsedMilliseconds} ms | {ms / Frame:f2} ms | {ms / threads.Length / 1000:f2} s";
                            }
                            mutex.WaitOne();
                            bar.Update(Frame);
                            mutex.ReleaseMutex();
                        }
                    });
                    threads[threadID] = currentThread;
                    currentThread.Start();
                }

                foreach (Thread thread in threads)
                {
                    thread.Join();
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
    
        public static void PrintAsync(string filename, short fontSize)
        {
            Console.CursorVisible = false;
            ExtendedConsole.SetFont(fontSize);

            Stack<byte[]> frames;
            const int preRenderAmount = 50;

            using (var videoFrameReader = new VideoFrameReader(filename))
            {
                int totalFrames = (int)Math.Ceiling(videoFrameReader.FrameRate * videoFrameReader.Duration.TotalSeconds);
                frames = new(preRenderAmount);
                int framesRendered = 0;
                Thread renderThread = new(() =>
                {
                    foreach(var frame in videoFrameReader)
                    {
                        frames.Push(ImageToAscii.Convert(frame));
                        framesRendered++;

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
