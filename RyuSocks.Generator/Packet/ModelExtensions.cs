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

using System;

namespace RyuSocks.Generator.Packet
{
    internal static class ModelExtensions
    {
        public static string GetOffset(this PacketFieldModel model)
        {
            if (model.Offset > PacketFieldAttributeData.Default.Offset)
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
            if (model.Length > PacketFieldAttributeData.Default.Length)
            {
                return model.Length.ToString();
            }

            if (model.LengthMember.Length == 0)
            {
                throw new InvalidOperationException($"No {nameof(model.LengthMember)} found.");
            }

            return $"this.{model.LengthMember}";
        }
    }
}
