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

        private static string[] Generate(PacketFieldModel packetField, bool isGetter)
        {
            BlockBuilder source = new();
            source.EnterScope(isGetter ? $"{packetField.PropertyGetterModifiers}get" : $"{packetField.PropertySetterModifiers}set");

            if (packetField.FieldType.IsArray)
            {
                // TODO: Deal with arrays
                //       Arrays could contain structs, enums or classes
                source.AppendBlock(GenerateArrayAccessor(packetField, isGetter));
            }
            else if (packetField.FieldType.IsStruct)
            {
                source.AppendBlock(GenerateStructAccessor(packetField, isGetter));
            }
            // Deal with simple types: enums, strings and integral numeric types
            else if (packetField.FieldType is { ActualType: ActualType.NamedType, Name: "string" }
                || packetField.FieldType.ActualType != ActualType.NamedType)
            {
                source.AppendBlock(GenerateSimpleAccessor(packetField, isGetter));
            }
            else if (packetField.FieldType.ActualType == ActualType.NamedType)
            {
                source.AppendBlock(GenerateClassAccessor(packetField, isGetter));
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
