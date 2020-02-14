using System.Collections.Generic;

namespace NGIMU.Scripts
{
    public static class Console
    {
        public delegate void ConsoleChangedHandler(string[] lines);

        private const int NumberOfLinesToBuffer = 80;

        private static readonly object SyncObject = new object();

        private static readonly Queue<string> LineBuffer = new Queue<string>(NumberOfLinesToBuffer);

        public static string[] Lines { get; private set; }

        public static event ConsoleChangedHandler LinesChanged;

        public static void Clear()
        {
            lock (SyncObject)
            {
                LineBuffer.Clear();

                UpdateLines();
            }
        }

        public static void Print(string line)
        {
            lock (SyncObject)
            {
                if (LineBuffer.Count >= NumberOfLinesToBuffer)
                {
                    LineBuffer.Dequeue();
                }

                LineBuffer.Enqueue(line);

                UpdateLines();
            }
        }

        private static void UpdateLines()
        {
            Lines = LineBuffer.ToArray();

            LinesChanged?.Invoke(Lines);
        }
    }
}