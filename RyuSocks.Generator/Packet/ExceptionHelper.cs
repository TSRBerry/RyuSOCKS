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
    // NOTE: Required for compatability with .NET Standard and .NET < 8
    internal static class ExceptionHelper
    {
        public static class ArgumentOutOfRange
        {
            public static void GenerateThrowIfNotEqual(BlockBuilder source, string value, string other)
            {
                string message = $"$\"{{nameof({value})}} must be equal to: {{{other}}}\"";
                source.EnterScope($"if ({value} != {other})");
                source.AppendLine($"throw new ArgumentOutOfRangeException(nameof({value}), {value}, {message});");
                source.LeaveScope();
            }

            public static void GenerateThrowIfLessThan(BlockBuilder source, string value, string other)
            {
                string message = $"$\"{{nameof({value})}} must be larger or equal to: {{{other}}}\"";
                source.EnterScope($"if ({value} < {other})");
                source.AppendLine($"throw new ArgumentOutOfRangeException(nameof({value}), {value}, {message});");
                source.LeaveScope();
            }

            public static void GenerateThrowIfGreaterThan(BlockBuilder source, string value, string other)
            {
                string message = $"$\"{{nameof({value})}} must be smaller or equal to: {{{other}}}\"";
                source.EnterScope($"if ({value} > {other})");
                source.AppendLine($"throw new ArgumentOutOfRangeException(nameof({value}), {value}, {message});");
                source.LeaveScope();
            }
        }
    }
}
