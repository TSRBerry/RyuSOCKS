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

using RyuSocks.Types;
using System.Net.Sockets;

namespace RyuSocks.Utils
{
    public static class SocketErrorExtension
    {
        public static ReplyField ToReplyField(this SocketError error)
        {
            return error switch
            {
                SocketError.Success => ReplyField.Succeeded,
                SocketError.ConnectionRefused => ReplyField.ConnectionRefused,
                SocketError.HostUnreachable => ReplyField.HostUnreachable,
                SocketError.NetworkUnreachable => ReplyField.NetworkUnreachable,
                _ => ReplyField.ServerFailure,
            };
        }
    }
}
