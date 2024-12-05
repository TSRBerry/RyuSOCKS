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
    internal static partial class AccessorGenerator
    {
        private static void AddVerificationMethodIfNecessary(BlockBuilder source, PacketFieldModel packetField, bool isGetter)
        {
            if (packetField.ValidationMethodName.Length > 0)
            {
                string maybeValueParam = isGetter ? string.Empty : "value";

                source.AppendLine($"this.{packetField.ValidationMethodName}({maybeValueParam});");
            }
        }

        // TODO: Find a way to deal with classes and structs properly
        //       This probably requires a model change
        //       For classes:
        //         - getter: Require a specific constructor?
        //         - setter: Require a specific method/property to get access to a Span<byte> or byte[]?
        private static string[] Generate(PacketFieldModel packetField, bool isGetter)
        {
            BlockBuilder source = new();
            source.EnterScope(isGetter ? "get" : "set");

            if (packetField.FieldType.IsArray)
            {
                // TODO: Deal with arrays
                //       Arrays could contain structs, enums or classes
                source.AppendBlock(GenerateArrayAccessor(packetField, isGetter));
            }
            else if (packetField.FieldType.IsStruct)
            {
                // TODO: Deal with structs
                source.AppendLine("// TODO: structs");
            }
            // Deal with simple types: enums, strings and integral numeric types
            else if (packetField.FieldType is { ActualType: ActualType.NamedType, Name: "string" }
                || packetField.FieldType.ActualType != ActualType.NamedType)
            {
                source.AppendBlock(GenerateSimpleAccessor(packetField, isGetter));
            }
            else if (packetField.FieldType.ActualType == ActualType.NamedType)
            {
                // TODO: Deal with classes
                source.AppendLine("// TODO: classes");
            }
            else
            {
                throw new InvalidOperationException($"Unknown property type. Unable to generate accessor for: {packetField.FieldType.Name}({packetField.FieldType.ActualType})");
            }

            source.LeaveScope();
            return source.GetLines();
        }

        public static string[] GenerateGetter(PacketFieldModel packetField) => Generate(packetField, true);

        public static string[] GenerateSetter(PacketFieldModel packetField) => Generate(packetField, false);
    }
}
