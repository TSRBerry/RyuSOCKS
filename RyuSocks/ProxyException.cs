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
using System.Collections.Generic;

namespace RyuSocks
{
    public class ProxyException : Exception
    {
        private static readonly Dictionary<ReplyField, string> _exceptionMessages = new()
        {
            { ReplyField.ServerFailure, "The proxy server failed to process this request." },
            { ReplyField.ConnectionNotAllowed, "The proxy server did not allow the connection." },
            { ReplyField.NetworkUnreachable, "The target network is unreachable." },
            { ReplyField.HostUnreachable, "The target is unreachable." },
            { ReplyField.ConnectionRefused, "The target refused the connection" },
            { ReplyField.TTLExpired, "The TTL expired before reaching the target." },
            { ReplyField.CommandNotSupported, "The specified command is not supported." },
            { ReplyField.AddressTypeNotSupported, "The specified address type is not supported." },
        };

        public ReplyField ReplyCode { get; }

        public ProxyException(ReplyField replyField) : base($"{_exceptionMessages[replyField]} ({replyField})")
        {
            ReplyCode = replyField;
        }

        public ProxyException(ReplyField replyField, string message) : base(message)
        {
            ReplyCode = replyField;
        }

        public ProxyException(ReplyField replyField, string message, Exception innerException) : base(message,
            innerException)
        {
            ReplyCode = replyField;
        }
    }
}
