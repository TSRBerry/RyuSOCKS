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
            { ActualType.Int16, new TypeConverterModel(sizeof(short), "BinaryPrimitives.ReadInt16", "BinaryPrimitives.WriteInt16") },
            { ActualType.UInt16, new TypeConverterModel(sizeof(ushort), "BinaryPrimitives.ReadUInt16", "BinaryPrimitives.WriteUInt16") },
            { ActualType.Int32, new TypeConverterModel(sizeof(int), "BinaryPrimitives.ReadInt32", "BinaryPrimitives.WriteInt32") },
            { ActualType.UInt32, new TypeConverterModel(sizeof(uint), "BinaryPrimitives.ReadUInt32", "BinaryPrimitives.WriteUInt32") },
            { ActualType.Int64, new TypeConverterModel(sizeof(long), "BinaryPrimitives.ReadInt64", "BinaryPrimitives.WriteInt64") },
            { ActualType.UInt64, new TypeConverterModel(sizeof(ulong), "BinaryPrimitives.ReadUInt64", "BinaryPrimitives.WriteUInt64") },
        }.ToImmutableDictionary();
    }

    internal record struct TypeConverterModel(int Length, string ReaderMethodName, string WriterMethodName)
    {
        public string GetReaderName(bool isBigEndian)
        {
            return isBigEndian ? $"{ReaderMethodName}BigEndian" : $"{ReaderMethodName}LittleEndian";
        }

        public string GetWriterName(bool isBigEndian)
        {
            return isBigEndian ? $"{WriterMethodName}BigEndian" : $"{WriterMethodName}LittleEndian";
        }
    }
}
