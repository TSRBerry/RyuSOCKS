using System.Collections.Generic;

namespace RyuSocks.Generator
{
    class BlockBuilder : AbstractBuilder
    {
        private readonly List<string> _block = [];

        public override void AppendLine()
        {
            _block.Add(string.Empty);
        }

        public override void AppendLine(string text)
        {
            _block.Add(GetIndentedString(text));
        }

        public string[] GetLines()
        {
            return _block.ToArray();
        }

        private string GetIndentedString(string text = "")
        {
            string indent = "";

            for (int i = 0; i < IndentLength * CurrentIndentCount; i++)
            {
                indent += ' ';
            }

            return indent + text;
        }

        public override string ToString()
        {
            return string.Join("\n", _block);
        }
    }
}
