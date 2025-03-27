/*
 * Copyright (C) RyuSOCKS
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2,
 * as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using RyuSocks.Generator.Builder;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RyuSocks.Generator.Test.Builder
{
    [ExcludeFromCodeCoverage]
    public class BlockBuilderTests : AbstractBuilderTests<BlockBuilder>
    {
        [Theory]
        [InlineData(3, "abc", "// a comment", "")]
        [InlineData(1, "", "a statement;", "a string")]
        [InlineData(0, "// test", "this")]
        public void GetLines_ReturnsAllLines(int indentLevel, params string[] blockLines)
        {
            // Create indent string for the block
            string blockIndentString = "";
            for (int i = 0; i < indentLevel; i++)
            {
                blockIndentString += ExpectedIndentString;
            }

            // Add block lines to expectedArray
            string[] expectedArray = new string[blockLines.Length];
            for (int i = 0; i < blockLines.Length; i++)
            {
                expectedArray[i] = $"{blockIndentString}{blockLines[i]}";
            }

            var builder = new BlockBuilder();

            for (int i = 0; i < indentLevel; i++)
            {
                builder.IncreaseIndentation();
            }

            foreach (var line in blockLines)
            {
                builder.AppendLine(line);
            }

            Assert.Equal(expectedArray, builder.GetLines());
        }
    }
}
