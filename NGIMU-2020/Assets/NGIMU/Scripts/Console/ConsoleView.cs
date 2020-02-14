using TMPro;
using UnityEngine;

namespace NGIMU.Scripts
{
    public class ConsoleView : MonoBehaviour
    {
        public TMP_Text TextArea;

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
            if (linesHaveChanged == false)
            {
                return;
            }

            linesHaveChanged = false;

            string[] currentLines = lines;

            TextArea.text = currentLines == null ? "" : string.Join("\n", currentLines);
        }

        private void UpdateConsoleText(string[] newLines)
        {
            lines = newLines;
            linesHaveChanged = true;
        }
    }
}