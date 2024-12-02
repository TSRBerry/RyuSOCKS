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

using System.Collections.Generic;
using System.Collections.Immutable;

namespace RyuSocks.Generator.Packet
{
    internal static class TypeConverter
    {
        public static readonly ImmutableDictionary<ActualType, TypeConverterModel> Map = new Dictionary<ActualType, TypeConverterModel>
        {
            { ActualType.Int16, new TypeConverterModel(sizeof(short), "BitConverter.ToInt16") },
            { ActualType.UInt16, new TypeConverterModel(sizeof(ushort), "BitConverter.ToUInt16") },
            { ActualType.Int32, new TypeConverterModel(sizeof(int), "BitConverter.ToInt32") },
            { ActualType.UInt32, new TypeConverterModel(sizeof(uint), "BitConverter.ToUInt32") },
            { ActualType.Int64, new TypeConverterModel(sizeof(long), "BitConverter.ToInt64") },
            { ActualType.UInt64, new TypeConverterModel(sizeof(ulong), "BitConverter.ToUInt64") },
        }.ToImmutableDictionary();
    }

    internal record struct TypeConverterModel(int Length, string ConverterMethodName);
}
