using GleamTech.VideoUltimate;
using System.Diagnostics;

namespace ExtendedConsole
{
    public static class VideoToAscii
    {
        public static List<string[]> Convert(string filename)
        {
            List<string[]> Frames = new();
            int i = 0;
            using (var videoFrameReader = new VideoFrameReader(filename))
            {
                var watch = new Stopwatch();
                foreach(var frame in videoFrameReader)
                {
                    watch.Restart();
                    Frames.Add(ImageToAscii.Convert(frame));
                    i++; 
                    Console.Title = $"{i} | {watch.ElapsedMilliseconds} ms";
                }
            }
            return Frames;
        }
    }
}
