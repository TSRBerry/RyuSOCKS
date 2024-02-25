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

namespace RyuSocks.Commands.Client
{
    public class ConnectCommand : IClientCommand
    {
        public static ProxyCommand Id => ProxyCommand.Connect;

        private readonly SocksClient _parent;

        public ConnectCommand(SocksClient parent)
        {
            _parent = parent;
        }

        public void SendAsync(Span<byte> buffer)
        {
            _parent.SendAsync(buffer);
        }

        public byte[] Receive(byte[] buffer, long offset, long size)
        {
            return buffer;
        }
    }
}
