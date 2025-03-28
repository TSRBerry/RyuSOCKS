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
