namespace ExtendedConsole
{
    public class ProgressBar
    {
        private static int Length { get { return Console.BufferWidth - 2; } }
        private char ProgressChar { get; }
        private char PlaceholderChar { get; }
        private double Progress { get; set; }
        private double Total { get; set; }
        private ConsoleColor ProgressColor { get; }
        private ConsoleColor PlaceholderColor { get; }

        public ProgressBar(double total, char progressChar = '#', char placeholderChar = '-')
        {
            ProgressChar = progressChar;
            PlaceholderChar = placeholderChar;
            Total = total;
            Progress = 0;
        }

        public ProgressBar(double total, char progressChar = '#', char placeholderChar = '-', ConsoleColor progressColor = ConsoleColor.Green, ConsoleColor placeholderColor = ConsoleColor.White) : this(total, progressChar, placeholderChar)
        {
            ProgressColor = progressColor;
            PlaceholderColor = placeholderColor;
        }

        public void Update(double progress)
        {
            Console.SetCursorPosition(0, 0);
            Progress = progress;
            PrintProgressBar();
        }

        private void PrintProgressBar()
        {
            if (Progress > Total) return;

            // Amount of # in the ProgressBar
            double mask = (double)Progress / Total * Length;

            Console.ForegroundColor = PlaceholderColor;
            Console.Write("[");

            Console.ForegroundColor = ProgressColor;
            Console.Write(new string(ProgressChar, (int)mask));

            Console.ForegroundColor = PlaceholderColor;
            Console.Write($"{new string(PlaceholderChar, Length - (int)mask)}]\n");
        }
    }
}
