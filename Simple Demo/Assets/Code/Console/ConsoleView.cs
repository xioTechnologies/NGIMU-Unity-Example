using UnityEngine;
using UnityEngine.UI;

public class ConsoleView : MonoBehaviour
{
    public Text TextArea;
    public GameObject ViewContainer;

    private readonly object syncObject = new object();
    private string[] lines = new string[0];
    private bool linesHaveChanged = false;

    private void OnDestroy()
    {
        Console.LinesChanged -= OnLinesChanged;
    }

    private void OnLinesChanged(string[] lines)
    {
        UpdateConsoleText(lines);
    }

    private void Start()
    {
        Console.LinesChanged += OnLinesChanged;

        UpdateConsoleText(Console.Lines);
    }

    private void Update()
    {
        lock (syncObject)
        {
            if (linesHaveChanged == false)
            {
                return;
            }

            linesHaveChanged = false;

            if (lines == null)
            {
                TextArea.text = "";
            }
            else
            {
                TextArea.text = string.Join("\n", lines);
            }
        }
    }

    private void UpdateConsoleText(string[] newLines)
    {
        lock (syncObject)
        {
            lines = newLines;
            linesHaveChanged = true;
        }
    }
}