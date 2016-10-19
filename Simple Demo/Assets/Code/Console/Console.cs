using System.Collections.Generic;

public static class Console
{
    public delegate void ConsoleChangedHandler(string[] lines);

    private const int numberOfLinesToBuffer = 80;

    private static readonly object syncObject = new object(); 

    private static Queue<string> lineBuffer = new Queue<string>(numberOfLinesToBuffer);

    public static string[] Lines { get; private set; }

    public static event ConsoleChangedHandler LinesChanged;

    public static void Clear()
    {
        lock (syncObject)
        {
            lineBuffer.Clear();

            UpdateLines();
        }
    }

    public static void Print(string line)
    {
        lock (syncObject)
        {
            if (lineBuffer.Count >= numberOfLinesToBuffer)
            {
                lineBuffer.Dequeue();
            }

            lineBuffer.Enqueue(line);

            UpdateLines();
        }
    }

    private static void UpdateLines()
    {
        Lines = lineBuffer.ToArray();

        if (LinesChanged != null)
        {
            LinesChanged(Lines);
        }
    }
}