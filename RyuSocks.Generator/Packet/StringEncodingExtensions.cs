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

namespace RyuSocks.Generator.Packet
{
    internal static class StringEncodingExtensions
    {
        public static string GetByteCountText(this StringEncoding stringEncoding, string sourceArgText)
        {
            return $"Encoding.{stringEncoding}.GetByteCount({sourceArgText})";
        }

        public static string[] GetBytesText(this StringEncoding stringEncoding, string sourceArgText, string destinationArgText, bool isBigEndian)
        {
            if (isBigEndian)
            {
                switch (stringEncoding)
                {
                    case StringEncoding.Unicode:
                        return [$"Encoding.BigEndianUnicode.GetBytes({sourceArgText}, {destinationArgText});"];
                    case StringEncoding.UTF32:
                        BlockBuilder source = new();

                        // Write bytes in little endian to temp array
                        source.AppendLine($"byte[] byteArray = Encoding.UTF32.GetBytes({sourceArgText});");
                        source.AppendLine("Array.Reverse(byteArray);");
                        // Write bytes with correct endianness to the packet
                        source.AppendLine($"byteArray.CopyTo({destinationArgText});");

                        return source.GetLines();
                }
            }

            return [$"Encoding.{stringEncoding}.GetBytes({sourceArgText}, {destinationArgText});"];
        }

        public static string[] GetStringText(this StringEncoding stringEncoding, string sourceArgText, bool isBigEndian, bool returnValue = true)
        {
            string returnOrAssignText = returnValue ? "return" : "string valueString =";

            if (isBigEndian)
            {
                switch (stringEncoding)
                {
                    case StringEncoding.Unicode:
                        return [$"{returnOrAssignText} Encoding.BigEndianUnicode.GetString({sourceArgText});"];
                    case StringEncoding.UTF32:
                        BlockBuilder source = new();

                        // Get a temp array, so the original packet doesn't get modified.
                        source.AppendLine($"byte[] stringArray = {sourceArgText}.ToArray();");
                        source.AppendLine("Array.Reverse(stringArray);");
                        source.AppendLine($"{returnOrAssignText} Encoding.UTF32.GetString(stringArray);");

                        return source.GetLines();
                }
            }

            return [$"{returnOrAssignText} Encoding.{stringEncoding}.GetString({sourceArgText});"];
        }
    }
}
