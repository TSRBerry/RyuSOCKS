using System.Text;

namespace RyuSocks.Generator.Builder
{
    // Original source: https://github.com/Ryujinx/Ryujinx/blob/1df6c07f78c4c3b8c7fc679d7466f79a10c2d496/src/Ryujinx.Horizon.Generators/CodeGenerator.cs
    public class CodeBuilder : AbstractBuilder
    {
        private readonly StringBuilder _sb = new();

        public override void AppendLine()
        {
            _sb.AppendLine();
        }

        public override void AppendLine(string text)
        {
            _sb.Append(' ', IndentLength * CurrentIndentLevel);
            _sb.AppendLine(text);
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
