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
using RyuSocks.Commands.Extensions;
using RyuSocks.Packets;
using RyuSocks.Types;
using RyuSocks.Utils;
using System;
using System.Linq;
using System.Net;
using System.Security.Authentication;

namespace RyuSocks
{
    public partial class SocksSession : TcpSession
    {
        protected bool IsClosing;
        protected bool IsAuthenticated;
        protected IProxyAuth Auth;
        protected ServerCommand Command;

        public new SocksServer Server => base.Server as SocksServer;

        public SocksSession(TcpServer server) : base(server) { }

        private bool IsDestinationValid(CommandRequest request)
        {
            if (!Server.UseAllowList && !Server.UseBlockList)
            {
                return true;
            }

            bool isDestinationValid = false;

            // Check whether the client is allowed to connect to the requested destination.
            foreach (IPAddress destinationAddress in request.ProxyEndpoint.Addresses)
            {
                if (Server.UseAllowList
                    && Server.AllowedDestinations.TryGetValue(destinationAddress, out ushort[] allowedPorts)
                    && allowedPorts.Contains(request.DestinationPort))
                {
                    isDestinationValid = true;
                    break;
                }

                if (Server.UseBlockList
                    && (!Server.BlockedDestinations.TryGetValue(destinationAddress, out allowedPorts)
                        || !allowedPorts.Contains(request.DestinationPort)))
                {
                    isDestinationValid = true;
                    break;
                }
            }

            return isDestinationValid;
        }

        protected virtual void ProcessAuthMethodSelection(ReadOnlySpan<byte> buffer)
        {
            var request = new MethodSelectionRequest(buffer.ToArray());
            request.Validate();

            foreach (var requestedAuthMethod in request.Methods)
            {
                if (Server.AcceptableAuthMethods.Contains(requestedAuthMethod))
                {
                    var reply = new MethodSelectionResponse(requestedAuthMethod)
                    {
                        Version = ProxyConsts.Version,
                    };

                    SendAsync(reply.AsSpan());
                    Auth = requestedAuthMethod.GetAuth();

                    return;
                }
            }

            var errorReply = new MethodSelectionResponse(AuthMethod.NoAcceptableMethods)
            {
                Version = ProxyConsts.Version,
            };

            SendAsync(errorReply.AsSpan());
            IsClosing = true;
        }

        protected virtual void ProcessCommandRequest(ReadOnlySpan<byte> buffer)
        {
            var request = new CommandRequest(buffer.ToArray());
            request.Validate();

            var errorReply = new CommandResponse(new IPEndPoint(0, 0))
            {
                Version = ProxyConsts.Version,
            };

            // Check whether the client requested a valid command.
            if (Server.OfferedCommands.Contains(request.Command))
            {
                if (IsDestinationValid(request))
                {
                    Command = request.Command.GetServerCommand()(this, (IPEndPoint)Server.Endpoint, request.ProxyEndpoint);
                    return;
                }

                errorReply.ReplyField = ReplyField.ConnectionNotAllowed;
                SendAsync(errorReply.AsSpan());
                IsClosing = true;

                return;
            }

            errorReply.ReplyField = ReplyField.CommandNotSupported;
            SendAsync(errorReply.AsSpan());
            IsClosing = true;
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            ReadOnlySpan<byte> bufferSpan = buffer.AsSpan((int)offset, (int)size);

            // Choose the authentication method.
            if (Auth == null)
            {
                ProcessAuthMethodSelection(bufferSpan);
                IsAuthenticated = Auth.GetAuth() == AuthMethod.NoAuth;

                return;
            }

            // Authenticate the client.
            if (!IsAuthenticated)
            {
                try
                {
                    IsAuthenticated = Auth.Authenticate(bufferSpan, out ReadOnlySpan<byte> sendBuffer);
                    SendAsync(sendBuffer);
                }
                catch (AuthenticationException)
                {
                    // TODO: Log the exception here.
                    IsClosing = true;
                }

                return;
            }

            bufferSpan = Auth.Unwrap(bufferSpan);

            // Attempt to process a command request.
            if (Command == null)
            {
                ProcessCommandRequest(bufferSpan);
                return;
            }

            // Don't process packets for clients we are disconnecting soon.
            if (IsClosing)
            {
                return;
            }

            bufferSpan = Command.Unwrap(bufferSpan);

            Command.OnReceived(bufferSpan);
        }

        protected override void OnEmpty()
        {
            if (IsClosing)
            {
                Disconnect();
            }
        }

        public override bool SendAsync(ReadOnlySpan<byte> buffer)
        {
            if (Command != null)
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
            if (Command != null)
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
