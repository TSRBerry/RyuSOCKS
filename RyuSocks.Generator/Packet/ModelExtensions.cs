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

using Microsoft.CodeAnalysis;
using System;

namespace RyuSocks.Generator.Packet
{
    internal static class ModelExtensions
    {
        public static string GetOffset(this PacketFieldModel model)
        {
            if (model.Offset >= 0)
            {
                return model.Offset.ToString();
            }

            if (model.OffsetMember.Length == 0)
            {
                throw new InvalidOperationException($"No {nameof(model.OffsetMember)} found.");
            }

            return $"this.{model.OffsetMember}";
        }

        public static string GetLength(this PacketFieldModel model)
        {
            if (model.Length >= 0)
            {
                return model.Length.ToString();
            }

            if (model.LengthMember.Length == 0)
            {
                throw new InvalidOperationException($"No {nameof(model.LengthMember)} found.");
            }

            return $"this.{model.LengthMember}";
        }

        public static string ToModifierString(this Accessibility accessModifier)
        {
            return accessModifier switch
            {
                Accessibility.NotApplicable => string.Empty,
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal => "protected internal",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.Public => "public",
                _ => throw new InvalidOperationException($"Couldn't get access modifier string for: {accessModifier}"),
            };
        }
    }
}
