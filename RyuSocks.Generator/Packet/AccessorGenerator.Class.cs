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
        private static string[] GenerateIPAddressAccessor(PacketFieldModel packetField, bool isGetter)
        {
            BlockBuilder source = new();

            AddVerificationMethodIfNecessary(source, packetField, isGetter);

            if (isGetter)
            {
                source.AppendLine($"return new System.Net.IPAddress(this.AsSpan({packetField.GetOffset()}, {packetField.GetLength()}));");
            }
            else
            {
                source.AppendLine($"value.GetAddressBytes().CopyTo(this.AsSpan({packetField.GetOffset()}, {packetField.GetLength()}));");
            }

            return source.GetLines();
        }

        private static string[] GenerateClassAccessor(PacketFieldModel packetField, bool isGetter)
        {
            switch (packetField.FieldType.Name)
            {
                case "System.Net.IPAddress":
                    return GenerateIPAddressAccessor(packetField, isGetter);
                default:
                    // TODO: Deal with classes without relying on special handling
                    //       - getter: Require a specific constructor?
                    //       - setter: Require a specific method/property to get access to a Span<byte> or byte[]?
                    throw new InvalidOperationException($"Unknown property class type. Unable to generate accessor for: {packetField.FieldType.Name}({packetField.FieldType.ActualType})");
            }
        }
    }
}
