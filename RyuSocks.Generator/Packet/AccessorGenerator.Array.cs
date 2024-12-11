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
                    var converter = TypeConverter.Map[packetField.FieldType.ActualType];
                    source.AppendLine(GenerateArrayIntegralAccessor(packetField, converter, isGetter));
                    break;
                case ActualType.NamedType:
                // Only deal with strings here
                // TODO: Deal with strings
                // return GenerateArrayStringAccessor(packetField, isGetter);
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

        private static string[] GenerateArrayAccessor(PacketFieldModel packetField, bool isGetter)
        {
            BlockBuilder source = new();

            if (!isGetter)
            {
                if (packetField.MinLength > 0)
                {
                    ExceptionHelper.ArgumentOutOfRange.GenerateThrowIfLessThan(source, "value.Length", packetField.MinLength.ToString());
                }
                if (packetField.MaxLength > 0)
                {
                    ExceptionHelper.ArgumentOutOfRange.GenerateThrowIfGreaterThan(source, "value.Length", packetField.MaxLength.ToString());
                }
            }

            AddVerificationMethodIfNecessary(source, packetField, isGetter);

            if (!isGetter)
            {
                if (packetField is { Length: <= 0, LengthMemberPermissions: Permissions.ReadWrite })
                {
                    string maybeLengthMemberCast = packetField.LengthMemberType != ActualType.Int32 ? $"({packetField.LengthMemberType.ToTypeString()})" : string.Empty;
                    source.AppendLine($"this.{packetField.LengthMember} = {maybeLengthMemberCast}value.Length;");
                }
                else if (packetField is { Length: <= 0, LengthMemberPermissions: Permissions.ReadOnly })
                {
                    ExceptionHelper.ArgumentOutOfRange.GenerateThrowIfNotEqual(source, "value.Length", $"this.{packetField.LengthMember}");
                }

                source.AppendLine();
            }

            if (packetField.FieldType.IsStruct)
            {
                // TODO: Deal with struct arrays
                source.AppendLine("// TODO: struct arrays");
            }
            // Deal with simple types: enums, strings and integral numeric types
            else if (packetField.FieldType is { ActualType: ActualType.NamedType, Name: "string" }
                     || packetField.FieldType.ActualType != ActualType.NamedType)
            {
                source.AppendBlock(GenerateSimpleArrayAccessor(packetField, isGetter));
            }
            else if (packetField.FieldType.ActualType == ActualType.NamedType)
            {
                // TODO: Deal with class arrays
                source.AppendLine("// TODO: class arrays");
            }
            else
            {
                throw new InvalidOperationException($"Unknown property type. Unable to generate array accessor for: {packetField.FieldType.Name}({packetField.FieldType.ActualType})");
            }

            return source.GetLines();
        }
    }
}
