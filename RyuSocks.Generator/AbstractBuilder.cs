namespace RyuSocks.Generator
{
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

        public abstract void AppendLine();
        public abstract void AppendLine(string text);
    }
}
