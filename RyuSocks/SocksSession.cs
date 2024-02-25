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

using NetCoreServer;
using System;

namespace RyuSocks
{
    public partial class SocksSession : TcpSession
    {
        // TODO: Keep track of connection state
        // TODO: Perform authentication first, switch state and then check the other TODOs

        // TODO: Put this in a source generator
        public SocksSession(TcpServer server) : base(server)
        {
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            // TODO: Call Unwrap of current auth method
            // TODO: Call Unwrap of current command
            // TODO: Call OnReceived of current command
        }

        public override bool SendAsync(ReadOnlySpan<byte> buffer)
        {
            // TODO: Call Wrap of current command
            // TODO: Call Wrap of current auth method
            // TODO: Call base.SendAsync() with the new buffer
            return base.SendAsync(buffer);
        }
    }
}
