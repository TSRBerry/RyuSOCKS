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

using RyuSocks.Utils;
using System;
using System.Net;

namespace RyuSocks.Commands.Server
{
    public interface IServerCommand : ICommand, IDisposable
    {
        /// <summary>
        /// Initialize the command with the provided destination endpoint
        /// </summary>
        /// <param name="destEndpoint">The destination endpoint</param>
        /// <param name="boundEndpoint">The bound endpoint on success</param>
        /// <returns>The reply field value corresponding to the result of this operation</returns>
        public ReplyField Initialize(EndPoint destEndpoint, out EndPoint boundEndpoint);
        public void SendAsync(Span<byte> buffer);
        public void OnReceived(object sender, ReceivedEventArgs args);
        public void Disconnect();
    }
}
