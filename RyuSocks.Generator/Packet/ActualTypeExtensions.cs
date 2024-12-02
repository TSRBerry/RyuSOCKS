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

using System;

namespace RyuSocks.Generator.Packet
{
    internal static class ActualTypeExtensions
    {
        public static string ToTypeString(this ActualType actualType)
        {
            return actualType switch
            {
                ActualType.SByte => "sbyte",
                ActualType.Byte => "byte",
                ActualType.Int16 => "short",
                ActualType.UInt16 => "ushort",
                ActualType.Int32 => "int",
                ActualType.UInt32 => "uint",
                ActualType.Int64 => "long",
                ActualType.UInt64 => "ulong",
                _ => throw new InvalidOperationException($"Couldn't get type string from {nameof(ActualType)}: {actualType}"),
            };
        }
    }
}
