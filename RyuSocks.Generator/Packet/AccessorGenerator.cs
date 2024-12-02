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

namespace RyuSocks.Generator.Packet
{
    internal static class AccessorGenerator
    {
        private static void GenerateSimpleStringAccessor(string packetBytesFieldName, CodeBuilder source, PacketFieldModel packetField, bool isGetter)
        {
            // FIXME: This implementation assumes strings are always ASCII
            if (isGetter)
            {
                if (packetField.FieldType.IsEnum)
                {
                    source.AppendLine($"Enum.Parse<{packetField.FieldType.Name}>(Encoding.ASCII.GetString({packetBytesFieldName}, {packetField.GetOffset()}, {packetField.GetLength()}), true);");
                    return;
                }

                source.AppendLine($"return Encoding.ASCII.GetString({packetBytesFieldName}, {packetField.GetOffset()}, {packetField.GetLength()});");
            }
            else
            {
                string valueParameter = packetField.FieldType.IsEnum
                    ? $"Enum.GetName<{packetField.FieldType.Name}>(value)"
                    : "value";

                if (packetField.Length <= 0)
                {
                    if (packetField.FieldType.IsEnum)
                    {
                        source.AppendLine($"string valueString = {valueParameter};");
                        valueParameter = "valueString";
                    }

                    // TODO: Add type cast if necessary
                    source.AppendLine($"this.{packetField.LengthMember} = {valueParameter}.Length;");
                    source.AppendLine($"Encoding.ASCII.GetBytes({valueParameter}, {packetBytesFieldName}.AsSpan({packetField.GetOffset()}, this.{packetField.LengthMember}));");
                }
                else
                {
                    source.AppendLine($"Encoding.ASCII.GetBytes({valueParameter}, {packetBytesFieldName}.AsSpan({packetField.GetOffset()}, {packetField.GetLength()}));");
                }
            }
        }

        private static void GenerateSimpleAccessor(string packetBytesFieldName, CodeBuilder source, PacketFieldModel packetField, bool isGetter)
        {
            string maybeCast = packetField.FieldType.IsEnum ? $"({packetField.FieldType.Name})" : string.Empty;

            switch (packetField.FieldType.ActualType)
            {
                case ActualType.Byte:
                    if (isGetter)
                    {
                        source.AppendLine($"return {maybeCast}{packetBytesFieldName}[{packetField.GetOffset()}];");
                    }
                    else
                    {
                        string maybeCastValue = packetField.FieldType.IsEnum ? "(byte)" : string.Empty;
                        source.AppendLine($"{packetBytesFieldName}[{packetField.GetOffset()}] = {maybeCastValue}value;");
                    }
                    break;
                case ActualType.SByte:
                    if (isGetter)
                    {
                        source.AppendLine($"return {maybeCast}(sbyte){packetBytesFieldName}[{packetField.GetOffset()}];");
                    }
                    else
                    {
                        source.AppendLine($"{packetBytesFieldName}[{packetField.GetOffset()}] = (byte)value;");
                    }
                    break;
                case ActualType.Int16:
                case ActualType.UInt16:
                case ActualType.Int32:
                case ActualType.UInt32:
                case ActualType.Int64:
                case ActualType.UInt64:
                    var converter = TypeConverter.Map[packetField.FieldType.ActualType];
                    if (isGetter)
                    {
                        if (packetField.IsBigEndian)
                        {
                            source.AppendLine($"Span<byte> valueSpan = {packetBytesFieldName}.AsSpan({packetField.GetOffset()}, {converter.Length});");
                            source.AppendLine("valueSpan.Reverse();");
                            source.AppendLine($"return {maybeCast}{converter.ConverterMethodName}(valueSpan);");
                        }
                        else
                        {
                            source.AppendLine($"return {maybeCast}{converter.ConverterMethodName}({packetBytesFieldName}.AsSpan({packetField.GetOffset()}, {converter.Length}));");
                        }
                    }
                    else
                    {
                        string valueParameter = packetField.FieldType.IsEnum
                            ? $"({packetField.FieldType.ActualType.ToTypeString()})value"
                            : "value";

                        if (packetField.IsBigEndian)
                        {
                            source.AppendLine($"byte[] valueBytes = BitConverter.GetBytes({valueParameter});");
                            source.AppendLine("Array.Reverse(valueBytes);");
                            source.AppendLine($"valueBytes.CopyTo({packetBytesFieldName}.AsSpan({packetField.GetOffset()}, {converter.Length}));");
                        }
                        else
                        {
                            source.AppendLine($"BitConverter.GetBytes({valueParameter}).CopyTo({packetBytesFieldName}.AsSpan({packetField.GetOffset()}, {converter.Length}));");
                        }
                    }
                    break;
                case ActualType.NamedType:
                    // Only deal with strings here
                    GenerateSimpleStringAccessor(packetBytesFieldName, source, packetField, isGetter);
                    break;
                default:
                    throw new InvalidOperationException($"Unable to generate simple accessor for type: {packetField.FieldType.Name}({packetField.FieldType.ActualType})");
            }

        }

        // TODO: Find a way to deal with classes and structs properly
        //       This probably requires a model change
        //       For classes:
        //         - getter: Require a specific constructor?
        //         - setter: Require a specific method/property to get access to a Span<byte> or byte[]?
        public static void Generate(string packetBytesFieldName, CodeBuilder source, PacketFieldModel packetField, bool isGetter)
        {
            if (packetField.FieldType.IsArray)
            {
                // TODO: Deal with arrays
                //       Arrays could contain structs, enums or classes
                source.AppendLine("// TODO: arrays");
                return;
            }

            if (packetField.FieldType.IsStruct)
            {
                // TODO: Deal with structs
                source.AppendLine("// TODO: structs");
                return;
            }

            // Deal with simple types: enums, strings and integral numeric types
            if (packetField.FieldType is { ActualType: ActualType.NamedType, Name: "String" }
                || packetField.FieldType.ActualType != ActualType.NamedType)
            {
                GenerateSimpleAccessor(packetBytesFieldName, source, packetField, isGetter);
                return;
            }

            if (packetField.FieldType.ActualType == ActualType.NamedType)
            {
                // TODO: Deal with classes
                source.AppendLine("// TODO: classes");
            }
        }
    }
}
