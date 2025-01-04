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
        private static string GenerateSimpleByteAccessor(PacketFieldModel packetField, bool isGetter)
        {
            if (isGetter)
            {
                string maybeCast = packetField.FieldType.IsEnum ? $"({packetField.FieldType.Name})" : string.Empty;

                return $"return {maybeCast}this[{packetField.GetOffset()}];";
            }
            else
            {
                string maybeCastValue = packetField.FieldType.IsEnum ? "(byte)" : string.Empty;

                return $"this[{packetField.GetOffset()}] = {maybeCastValue}value;";
            }
        }

        private static string GenerateSimpleSByteAccessor(PacketFieldModel packetField, bool isGetter)
        {
            if (isGetter)
            {
                string maybeCast = packetField.FieldType.IsEnum ? $"({packetField.FieldType.Name})" : string.Empty;

                return $"return {maybeCast}(sbyte)this[{packetField.GetOffset()}];";
            }
            else
            {
                return $"this[{packetField.GetOffset()}] = (byte)value;";
            }
        }

        private static string GenerateSimpleIntegralAccessor(PacketFieldModel packetField, TypeConverterModel converter, bool isGetter)
        {
            if (isGetter)
            {
                string maybeCast = packetField.FieldType.IsEnum ? $"({packetField.FieldType.Name})" : string.Empty;

                return $"return {maybeCast}{converter.GetReaderName(packetField.IsBigEndian)}(this.AsSpan({packetField.GetOffset()}, {converter.Length}));";
            }
            else
            {
                string valueParameter = packetField.FieldType.IsEnum ? $"({packetField.FieldType.ActualType.ToTypeString()})value" : "value";

                return $"{converter.GetWriterName(packetField.IsBigEndian)}(this.AsSpan({packetField.GetOffset()}, {converter.Length}), {valueParameter});";
            }
        }

        private static string[] GenerateSimpleStringAccessor(PacketFieldModel packetField, bool isGetter)
        {
            BlockBuilder source = new();

            if (isGetter)
            {
                AddVerificationMethodIfNecessary(source, packetField, true);

                source.AppendBlock(packetField.FieldStringEncoding.GetStringText($"this.AsSpan({packetField.GetOffset()}, {packetField.GetLength()})", packetField.IsBigEndian));
            }
            else
            {
                if (packetField.MinLength > 0)
                {
                    ExceptionHelper.ArgumentOutOfRange.GenerateThrowIfLessThan(source, "value.Length", packetField.MinLength.ToString());
                }
                if (packetField.MaxLength > 0)
                {
                    ExceptionHelper.ArgumentOutOfRange.GenerateThrowIfGreaterThan(source, "value.Length", packetField.MaxLength.ToString());
                }

                AddVerificationMethodIfNecessary(source, packetField, false);

                if (packetField is { Length: <= 0, LengthMemberPermissions: Permissions.ReadWrite })
                {
                    string maybeLengthMemberCast = packetField.LengthMemberType != ActualType.Int32 ? $"({packetField.LengthMemberType.ToTypeString()})" : string.Empty;

                    source.AppendLine($"this.{packetField.LengthMember} = {maybeLengthMemberCast}{packetField.FieldStringEncoding.GetByteCountText("value")};");
                    source.AppendBlock(packetField.FieldStringEncoding.GetBytesText("value", $"this.AsSpan({packetField.GetOffset()}, this.{packetField.LengthMember})", packetField.IsBigEndian));
                }
                else
                {
                    if (packetField is { Length: <= 0, LengthMemberPermissions: Permissions.ReadOnly })
                    {
                        ExceptionHelper.ArgumentOutOfRange.GenerateThrowIfNotEqual(source, packetField.FieldStringEncoding.GetByteCountText("value"), packetField.GetLength(), "value", "byte length of value");
                    }

                    source.AppendBlock(packetField.FieldStringEncoding.GetBytesText("value", $"this.AsSpan({packetField.GetOffset()}, {packetField.GetLength()})", packetField.IsBigEndian));
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
                    source.AppendLine(GenerateSimpleByteAccessor(packetField, isGetter));
                    break;
                case ActualType.SByte:
                    source.AppendLine(GenerateSimpleSByteAccessor(packetField, isGetter));
                    break;
                case ActualType.Int16:
                case ActualType.UInt16:
                case ActualType.Int32:
                case ActualType.UInt32:
                case ActualType.Int64:
                case ActualType.UInt64:
                    var converter = TypeConverter.Map[packetField.FieldType.ActualType];
                    source.AppendLine(GenerateSimpleIntegralAccessor(packetField, converter, isGetter));
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
