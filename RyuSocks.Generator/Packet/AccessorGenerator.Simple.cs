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
    internal static partial class AccessorGenerator
    {
        private static string[] GenerateSimpleByteAccessor(PacketFieldModel packetField, bool isGetter)
        {
            if (isGetter)
            {
                string maybeCast = packetField.FieldType.IsEnum ? $"({packetField.FieldType.Name})" : string.Empty;

                return [$"return {maybeCast}this[{packetField.GetOffset()}];"];
            }
            else
            {
                string maybeCastValue = packetField.FieldType.IsEnum ? "(byte)" : string.Empty;

                return [$"this[{packetField.GetOffset()}] = {maybeCastValue}value;"];
            }
        }

        private static string[] GenerateSimpleSByteAccessor(PacketFieldModel packetField, bool isGetter)
        {
            if (isGetter)
            {
                string maybeCast = packetField.FieldType.IsEnum ? $"({packetField.FieldType.Name})" : string.Empty;

                return [$"return {maybeCast}(sbyte)this[{packetField.GetOffset()}];"];
            }
            else
            {
                return [$"this[{packetField.GetOffset()}] = (byte)value;"];
            }
        }

        private static string[] GenerateSimpleIntegralAccessor(PacketFieldModel packetField, TypeConverterModel converter, bool isGetter)
        {
            if (isGetter)
            {
                string maybeCast = packetField.FieldType.IsEnum ? $"({packetField.FieldType.Name})" : string.Empty;

                return [$"return {maybeCast}{converter.ConverterMethodName}(this.AsSpan({packetField.GetOffset()}, {converter.Length}));"];
            }
            else
            {
                string valueParameter = packetField.FieldType.IsEnum ? $"({packetField.FieldType.ActualType.ToTypeString()})value" : "value";

                return [$"BitConverter.GetBytes({valueParameter}).CopyTo(this.AsSpan({packetField.GetOffset()}, {converter.Length}));"];
            }
        }

        private static string[] GenerateSimpleReversedIntegralAccessor(PacketFieldModel packetField, TypeConverterModel converter, bool isGetter)
        {
            BlockBuilder source = new();

            if (isGetter)
            {
                string maybeCast = packetField.FieldType.IsEnum ? $"({packetField.FieldType.Name})" : string.Empty;

                source.AppendLine($"Span<byte> valueSpan = this.AsSpan({packetField.GetOffset()}, {converter.Length});");
                source.AppendLine("valueSpan.Reverse();");
                source.AppendLine($"return {maybeCast}{converter.ConverterMethodName}(valueSpan);");
            }
            else
            {
                string valueParameter = packetField.FieldType.IsEnum ? $"({packetField.FieldType.ActualType.ToTypeString()})value" : "value";

                source.AppendLine($"byte[] valueBytes = BitConverter.GetBytes({valueParameter});");
                source.AppendLine("Array.Reverse(valueBytes);");
                source.AppendLine($"valueBytes.CopyTo(this.AsSpan({packetField.GetOffset()}, {converter.Length}));");
            }

            return source.GetLines();
        }

        private static string[] GenerateSimpleStringAccessor(PacketFieldModel packetField, bool isGetter)
        {
            BlockBuilder source = new();

            // FIXME: This implementation assumes strings are always ASCII
            if (isGetter)
            {
                AddVerificationMethodIfNecessary(source, packetField, true);

                if (packetField.FieldType.IsEnum)
                {
                    source.AppendLine($"Enum.Parse<{packetField.FieldType.Name}>(Encoding.ASCII.GetString(this.AsSpan({packetField.GetOffset()}, {packetField.GetLength()})), true);");

                    return source.GetLines();
                }

                source.AppendLine($"return Encoding.ASCII.GetString(this.AsSpan({packetField.GetOffset()}, {packetField.GetLength()}));");
            }
            else
            {
                string valueParameter = packetField.FieldType.IsEnum ? $"Enum.GetName<{packetField.FieldType.Name}>(value)" : "value";

                if (packetField.FieldType.IsEnum)
                {
                    source.AppendLine($"string valueString = {valueParameter};");
                    valueParameter = "valueString";
                }

                if (packetField.MinLength > 0)
                {
                    source.AppendLine($"ArgumentOutOfRangeException.ThrowIfLessThan({valueParameter}.Length, {packetField.MinLength});");
                }
                if (packetField.MaxLength > 0)
                {
                    source.AppendLine($"ArgumentOutOfRangeException.ThrowIfGreaterThan({valueParameter}.Length, {packetField.MaxLength});");
                }

                AddVerificationMethodIfNecessary(source, packetField, false);

                if (packetField.Length <= 0)
                {
                    string maybeLengthMemberCast = packetField.LengthMemberType != ActualType.Int32 ? $"({packetField.LengthMemberType.ToTypeString()})" : string.Empty;

                    source.AppendLine($"this.{packetField.LengthMember} = {maybeLengthMemberCast}{valueParameter}.Length;");
                    source.AppendLine($"Encoding.ASCII.GetBytes({valueParameter}, this.AsSpan({packetField.GetOffset()}, this.{packetField.LengthMember}));");
                }
                else
                {
                    source.AppendLine($"Encoding.ASCII.GetBytes({valueParameter}, this.AsSpan({packetField.GetOffset()}, {packetField.GetLength()}));");
                }
            }

            return source.GetLines();
        }

        private static string[] GenerateSimpleAccessor(PacketFieldModel packetField, bool isGetter)
        {
            BlockBuilder source = new();

            if (packetField.FieldType.ActualType != ActualType.NamedType)
            {
                AddVerificationMethodIfNecessary(source, packetField, isGetter);
            }

            switch (packetField.FieldType.ActualType)
            {
                case ActualType.Byte:
                    source.AppendBlock(GenerateSimpleByteAccessor(packetField, isGetter));
                    break;
                case ActualType.SByte:
                    source.AppendBlock(GenerateSimpleSByteAccessor(packetField, isGetter));
                    break;
                case ActualType.Int16:
                case ActualType.UInt16:
                case ActualType.Int32:
                case ActualType.UInt32:
                case ActualType.Int64:
                case ActualType.UInt64:
                    var converter = TypeConverter.Map[packetField.FieldType.ActualType];
                    var accessorBlock = packetField.IsBigEndian
                        ? GenerateSimpleReversedIntegralAccessor(packetField, converter, isGetter)
                        : GenerateSimpleIntegralAccessor(packetField, converter, isGetter);
                    source.AppendBlock(accessorBlock);
                    break;
                case ActualType.NamedType:
                    // Only deal with strings here
                    source.AppendBlock(GenerateSimpleStringAccessor(packetField, isGetter));
                    break;
                default:
                    throw new InvalidOperationException($"Unable to generate simple accessor for type: {packetField.FieldType.Name}({packetField.FieldType.ActualType})");
            }

            return source.GetLines();
        }
    }
}
