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
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RyuSocks.Generator.Test.Builder
{
    [ExcludeFromCodeCoverage]
    public abstract class AbstractBuilderTests<T> where T : AbstractBuilder, new()
    {
        protected const string ExpectedIndentString = "    ";

        private static string BuildExpectedString(string normalText, string prefixLine, string scopeText, string suffixText, int scopeAmount)
        {
            // Build the expected string
            string expectedString = $"{normalText}\n";
            string scopeIndentString = "";
            for (int i = 0; i < scopeAmount; i++)
            {
                // Add the prefix line
                if (prefixLine != null)
                {
                    expectedString += $"{scopeIndentString}{prefixLine}\n";
                }
                // Add the new scope line
                expectedString += $"{scopeIndentString}{{\n";
                // Increase the indent for the new scope
                scopeIndentString += ExpectedIndentString;
                // Add the scope text
                if (scopeText != string.Empty)
                {
                    expectedString += $"{scopeIndentString}{scopeText}\n";
                }
                else
                {
                    expectedString += "\n";
                }
            }
            // Leave the scopes
            while (scopeIndentString.Length != 0)
            {
                // Decrease the indent
                scopeIndentString = scopeIndentString[..^4];
                // Leave the scope
                expectedString += $"{scopeIndentString}}}{suffixText}\n";
            }

            return expectedString;
        }

        [Theory]
        [InlineData("abc", "TEST", "", "123", "\n", "aaaaaaaa")]
        [InlineData("", "TEST", "123", "", "aaaaaaaa")]
        [InlineData("\n", "ABC", "123", "", "aaaaaaaa", "")]
        public void AppendLine_KeepsEmptyLines(params string[] expectedLines)
        {
            T builder = new();

            foreach (var line in expectedLines)
            {
                if (line != string.Empty)
                {
                    builder.AppendLine(line);
                }
                else
                {
                    builder.AppendLine();
                }
            }

            Assert.Equal(string.Join("\n", expectedLines) + "\n", builder.ToString());
        }

        [Theory]
        [InlineData(2, "// A comment", "something else", "abc")]
        [InlineData(0, "something", "nothing")]
        [InlineData(1, "", "nothing", "")]
        public void AppendBlock_UsesIndentLevel(int indentLevel, params string[] blockLines)
        {
            // Create indent string for the block
            string blockIndentString = "";
            for (int i = 0; i < indentLevel; i++)
            {
                blockIndentString += ExpectedIndentString;
            }

            // Add block lines to expected string
            string expectedString = "";
            foreach (var line in blockLines)
            {
                expectedString += $"{blockIndentString}{line}\n";
            }

            T builder = new();

            for (int i = 0; i < indentLevel; i++)
            {
                builder.IncreaseIndentation();
            }

            builder.AppendBlock(blockLines);

            Assert.Equal(expectedString, builder.ToString());
        }

        [Theory]
        [InlineData("abc", "", "// A comment", ";", 3)]
        [InlineData("123", null, "statement;", ";", 1)]
        [InlineData("// The comment", "something123", "", "", 2)]
        public void Scope_UsesIncreasedIndentLevel(string normalText, string prefixLine, string scopeText, string suffixText, int scopeAmount)
        {
            string expectedString = BuildExpectedString(normalText, prefixLine, scopeText, suffixText, scopeAmount);

            T builder = new();
            builder.AppendLine(normalText);

            // Enter scopes
            for (int i = 0; i < scopeAmount; i++)
            {
                builder.EnterScope(prefixLine);

                if (scopeText != string.Empty)
                {
                    builder.AppendLine(scopeText);
                }
                else
                {
                    builder.AppendLine();
                }
            }
            // Leave scopes
            for (int i = 0; i < scopeAmount; i++)
            {
                if (suffixText != string.Empty)
                {
                    builder.LeaveScope(suffixText);
                }
                else
                {
                    builder.LeaveScope();
                }
            }

            Assert.Equal(expectedString, builder.ToString());
        }

        [Fact]
        public void LeaveScope_ThrowsWithoutIndention()
        {
            T builder = new();

            Assert.Throws<InvalidOperationException>(() => builder.LeaveScope());
        }

        [Fact]
        public void DecreaseIndention_ThrowsWithoutIndention()
        {
            T builder = new();

            Assert.Throws<InvalidOperationException>(() => builder.DecreaseIndentation());
        }

        [Fact]
        public void IncreaseAndDecreaseIndention_WorkAsExpected()
        {
            const string ExpectedString = "    TEST\nabc123\n        DEF\n\n    // END\n";
            T builder = new();

            builder.IncreaseIndentation();
            builder.AppendLine("TEST");
            builder.DecreaseIndentation();
            builder.AppendLine("abc123");
            builder.IncreaseIndentation();
            builder.IncreaseIndentation();
            builder.AppendLine("DEF");
            builder.AppendLine();
            builder.DecreaseIndentation();
            builder.AppendLine("// END");

            Assert.Equal(ExpectedString, builder.ToString());
        }
    }
}
