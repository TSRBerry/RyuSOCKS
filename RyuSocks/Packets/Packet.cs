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

namespace RyuSocks.Packets
{
    public abstract partial class Packet
    {
        /// <summary>
        /// Validate the structure of the packet.
        /// This method is not supposed to verify the contents of the packet in depth.
        /// </summary>
        public abstract void Validate();

        /// <summary>
        /// Check whether the structure of the packet is valid.
        /// This method calls <see cref="Validate"/> internally, but doesn't throw the exception on failure
        /// and returns a <see langword="bool"/> instead.
        /// </summary>
        public bool IsValid()
        {
            try
            {
                Validate();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
