using System.Diagnostics.CodeAnalysis;

namespace RyuSocks.Generator.Builder
{
    [ExcludeFromCodeCoverage]
    abstract class AbstractBuilder
    {
        protected const int IndentLength = 4;
        protected int CurrentIndentCount;

        public void EnterScope(string header = null)
        {
            if (header != null)
            {
                AppendLine(header);
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
            CurrentIndentCount++;
        }

        public void DecreaseIndentation()
        {
            if (CurrentIndentCount - 1 >= 0)
            {
                CurrentIndentCount--;
            }
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
