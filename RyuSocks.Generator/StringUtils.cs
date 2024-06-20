// Copyright (C) RyuSOCKS
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2,
// as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace RyuSocks.Generator
{
    public static class StringUtils
    {
        /// <summary>
        /// Wrapper for <see cref="string.Substring(int, int)"/> to handle negative lengths.
        /// </summary>
        /// <see cref="string.Substring(int, int)"/>
        public static string Extract(this string source, int startIndex, int length)
        {
            if (length >= 0)
            {
                return source.Substring(startIndex, length);
            }

            int finalLength = source.Length - startIndex + length;
            return source.Substring(startIndex, finalLength);
        }
    }
}
