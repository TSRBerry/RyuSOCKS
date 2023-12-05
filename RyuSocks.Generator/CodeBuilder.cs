using System.Text;

namespace RyuSocks.Generator
{
    // Source: https://github.com/Ryujinx/Ryujinx/blob/1df6c07f78c4c3b8c7fc679d7466f79a10c2d496/src/Ryujinx.Horizon.Generators/CodeGenerator.cs
    class CodeBuilder
    {
        private const int IndentLength = 4;

        private readonly StringBuilder _sb = new();
        private int _currentIndentCount;

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
            _currentIndentCount++;
        }

        public void DecreaseIndentation()
        {
            if (_currentIndentCount - 1 >= 0)
            {
                _currentIndentCount--;
            }
        }

        public void AppendLine()
        {
            _sb.AppendLine();
        }

        public void AppendLine(string text)
        {
            _sb.Append(' ', IndentLength * _currentIndentCount);
            _sb.AppendLine(text);
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
