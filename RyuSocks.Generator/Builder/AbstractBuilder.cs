using System;

namespace RyuSocks.Generator.Builder
{
    public abstract class AbstractBuilder
    {
        protected const int IndentLength = 4;
        protected int CurrentIndentLevel;

        public void EnterScope(string prefixLine = null)
        {
            if (prefixLine != null)
            {
                AppendLine(prefixLine);
            }

            AppendLine("{");
            IncreaseIndentation();
        }

        public void LeaveScope(string suffix = "")
        {
            DecreaseIndentation();
            AppendLine($"}}{suffix}");
        }

        public void IncreaseIndentation()
        {
            CurrentIndentLevel++;
        }

        public void DecreaseIndentation()
        {
            if (CurrentIndentLevel <= 0)
            {
                throw new InvalidOperationException(
                    $"Unable to decrease indent level further. {nameof(CurrentIndentLevel)}: {CurrentIndentLevel}");
            }

            CurrentIndentLevel--;
        }

        public void AppendBlock(string[] block)
        {
            foreach (var line in block)
            {
                AppendLine(line);
            }
        }

        public abstract void AppendLine();
        public abstract void AppendLine(string text);
    }
}
