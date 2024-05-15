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
using RyuSocks.Auth;
using RyuSocks.Auth.Extensions;
using RyuSocks.Commands;
using RyuSocks.Packets;
using RyuSocks.Types;
using RyuSocks.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace RyuSocks
{
    public partial class SocksClient : TcpClient
    {
        // TODO: Add (generated) properties for auth methods and commands
        // TODO: Keep track of the connection state
        // TODO: Expose events and methods to work with this client

        protected bool IsAuthenticated;
        protected bool IsCommandAccepted;
        protected ClientCommand Command;
        protected IProxyAuth Auth;

        public IReadOnlyDictionary<AuthMethod, IProxyAuth> OfferedAuthMethods { get; set; } = new Dictionary<AuthMethod, IProxyAuth>();

        public SocksClient(IPAddress address, ushort port = ProxyConsts.DefaultPort) : base(address, port) { }
        public SocksClient(string address, ushort port = ProxyConsts.DefaultPort) : base(address, port) { }
        public SocksClient(DnsEndPoint endpoint) : base(endpoint) { }
        public SocksClient(IPEndPoint endpoint) : base(endpoint) { }

        protected override void OnConnected()
        {
            var request = new MethodSelectionRequest(OfferedAuthMethods.Keys.ToArray())
            {
                Version = ProxyConsts.Version,
            };

            SendAsync(request.AsSpan());
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            ReadOnlySpan<byte> bufferSpan = buffer.AsSpan((int)offset, (int)size);

            if (Auth == null)
            {
                ProcessAuthMethodSelection(bufferSpan);
                IsAuthenticated = Auth?.GetAuth() == AuthMethod.NoAuth;

                return;
            }

            bufferSpan = Auth.Unwrap(bufferSpan);

            if (Command == null)
            {
                ProcessCommandResponse(bufferSpan);
                return;
            }

            bufferSpan = Command.Unwrap(bufferSpan);

            OnReceived(bufferSpan);
        }

        protected virtual void OnReceived(ReadOnlySpan<byte> buffer) { }

        protected virtual void ProcessAuthMethodSelection(ReadOnlySpan<byte> buffer)
        {
            var response = new MethodSelectionResponse(buffer.ToArray());
            response.Validate();

            Debug.Assert(OfferedAuthMethods.ContainsKey(response.Method));
            Auth = OfferedAuthMethods[response.Method];
        }

        protected virtual void ProcessCommandResponse(ReadOnlySpan<byte> buffer)
        {
            var response = new CommandResponse(buffer.ToArray());
            response.Validate();

            EnsureSuccessReply(response.ReplyField);
            IsCommandAccepted = true;

            // TODO: Notify command about destination from response
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void EnsureSuccessReply(ReplyField replyField)
        {
            if (replyField != ReplyField.Succeeded)
            {
                throw new ProxyException(replyField);
            }
        }

        public override bool SendAsync(ReadOnlySpan<byte> buffer)
        {
            if (Command != null && IsCommandAccepted)
            {
                buffer = Command.Wrap(buffer);
            }

            if (IsAuthenticated)
            {
                buffer = Auth.Wrap(buffer);
            }

            return base.SendAsync(buffer);
        }

        public override long Send(ReadOnlySpan<byte> buffer)
        {
            if (Command != null && IsCommandAccepted)
            {
                buffer = Command.Wrap(buffer);
            }

            if (IsAuthenticated)
            {
                buffer = Auth.Wrap(buffer);
            }

            return base.Send(buffer);
        }
    }
}
