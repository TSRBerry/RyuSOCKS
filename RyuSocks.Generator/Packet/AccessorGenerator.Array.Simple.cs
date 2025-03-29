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

namespace RyuSocks.Generator.Packet
{
    [SuppressMessage("ReSharper", "RedundantIfElseBlock")]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    internal static partial class AccessorGenerator
    {
        private static string GenerateArrayByteAccessor(PacketFieldModel packetField, bool isGetter)
        {
            if (isGetter)
            {
                string fieldTypeName = packetField.FieldType.Name.Extract(0, -2);
                string maybeCast = packetField.FieldType.IsEnum ? $"({fieldTypeName})" : string.Empty;

                return $"result[i] = {maybeCast}this[{packetField.GetOffset()} + i];";
            }
            else
            {
                string maybeCastValue = packetField.FieldType.IsEnum ? "(byte)" : string.Empty;

                return $"this[{packetField.GetOffset()} + i] = {maybeCastValue}value[i];";
            }
        }

        private static string GenerateArraySByteAccessor(PacketFieldModel packetField, bool isGetter)
        {
            if (isGetter)
            {
                string fieldTypeName = packetField.FieldType.Name.Extract(0, -2);

                return $"result[i] = ({fieldTypeName})this[{packetField.GetOffset()} + i];";
            }
            else
            {
                return $"this[{packetField.GetOffset()} + i] = (byte)value[i];";
            }
        }

        private static string GenerateArrayIntegralAccessor(PacketFieldModel packetField, TypeConverterModel converter, bool isGetter)
        {
            if (isGetter)
            {
                string fieldTypeName = packetField.FieldType.Name.Extract(0, -2);
                string maybeCast = packetField.FieldType.IsEnum ? $"({fieldTypeName})" : string.Empty;

                return $"result[i] = {maybeCast}{converter.GetReaderName(packetField.IsBigEndian)}(this.AsSpan({packetField.GetOffset()} + (i * {converter.Length}), {converter.Length}));";
            }
            else
            {
                string valueParameter = packetField.FieldType.IsEnum ? $"({packetField.FieldType.ActualType.ToTypeString()})value" : "value";

                return $"{converter.GetWriterName(packetField.IsBigEndian)}(this.AsSpan({packetField.GetOffset()} + (i * {converter.Length}), {converter.Length}), {valueParameter}[i]);";
            }
        }

        private static string[] GenerateArrayStringAccessor(PacketFieldModel packetField, bool isGetter)
        {
            if (isGetter)
            {
                return packetField.FieldStringEncoding.GetStringText($"this.AsSpan({packetField.GetOffset()} + (i * {packetField.FieldType.ElementSize}), {packetField.FieldType.ElementSize})", packetField.IsBigEndian, "result[i] =");
            }
            else
            {
                return packetField.FieldStringEncoding.GetBytesText("value[i]", $"this.AsSpan({packetField.GetOffset()} + (i * {packetField.FieldType.ElementSize}), {packetField.FieldType.ElementSize})", packetField.IsBigEndian);
            }
        }

        private static string[] GenerateSimpleArrayAccessor(PacketFieldModel packetField, bool isGetter)
        {
            BlockBuilder source = new();

            if (isGetter)
            {
                string fieldTypeName = packetField.FieldType.Name.Extract(0, -2);

                source.AppendLine($"{fieldTypeName}[] result = new {fieldTypeName}[{packetField.GetLength()}];");
                source.AppendLine();
            }

            source.EnterScope($"for (int i = 0; i < {packetField.GetLength()}; i++)");

            switch (packetField.FieldType.ActualType)
            {
                case ActualType.Byte:
                    // TODO: Provide access to the underlying array for the specified length.
                    //       Don't copy bytes to a new array.
                    source.AppendLine(GenerateArrayByteAccessor(packetField, isGetter));
                    break;
                case ActualType.SByte:
                    source.AppendLine(GenerateArraySByteAccessor(packetField, isGetter));
                    break;
                case ActualType.Int16:
                case ActualType.UInt16:
                case ActualType.Int32:
                case ActualType.UInt32:
                case ActualType.Int64:
                case ActualType.UInt64:
                    // TODO: Could this be improved with Spans?
                    var converter = TypeConverter.Map[packetField.FieldType.ActualType];
                    source.AppendLine(GenerateArrayIntegralAccessor(packetField, converter, isGetter));
                    break;
                case ActualType.NamedType:
                    // Only deal with strings here
                    source.AppendBlock(GenerateArrayStringAccessor(packetField, isGetter));
                    break;
                default:
                    throw new InvalidOperationException($"Unable to generate array accessor for type: {packetField.FieldType.Name}({packetField.FieldType.ActualType})");
            }

            source.LeaveScope();

            if (isGetter)
            {
                source.AppendLine();
                source.AppendLine("return result;");
            }

            return source.GetLines();
        }
    }
}
