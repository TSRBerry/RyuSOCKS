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

namespace RyuSocks.Generator.Packet
{
    // NOTE: Keep this in sync with the generated string encoding enum in PacketGenerator.cs
    internal enum StringEncoding : byte
    {
        ASCII,
        Unicode,
        UTF7,
        UTF8,
        UTF32,
    }
}
