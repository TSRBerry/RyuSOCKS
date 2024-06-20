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

        public bool IsDestinationValid(ProxyEndpoint destination)
        {
            if (!Server.UseAllowList && !Server.UseBlockList)
            {
                return true;
            }

            bool isDestinationValid = false;

            // Check whether the client is allowed to connect to the requested destination.
            foreach (IPAddress destinationAddress in destination.Addresses)
            {
                if (Server.UseAllowList
                    && Server.AllowedDestinations.TryGetValue(destinationAddress, out ushort[] allowedPorts)
                    && allowedPorts.Contains(destination.Port))
                {
                    isDestinationValid = true;
                    break;
                }

                if (Server.UseBlockList
                    && (!Server.BlockedDestinations.TryGetValue(destinationAddress, out allowedPorts)
                        || !allowedPorts.Contains(destination.Port)))
                {
                    isDestinationValid = true;
                    break;
                }
            }

            return isDestinationValid;
        }

        // TODO: Remove this once async Send/Receive for commands has been implemented.
        public Span<byte> Unwrap(Span<byte> packet, out ProxyEndpoint remoteEndpoint, out int wrapperLength)
        {
            if (!IsConnected || IsClosing)
            {
                throw new InvalidOperationException("Session is not connected or closing soon.");
            }

            remoteEndpoint = null;
            int totalWrapperLength = 0;

            if (IsAuthenticated)
            {
                packet = Auth.Unwrap(packet, out remoteEndpoint, out wrapperLength);
                totalWrapperLength += wrapperLength;
            }

            if (Command != null)
            {
                packet = Command.Unwrap(packet, out remoteEndpoint, out wrapperLength);
                totalWrapperLength += wrapperLength;
            }

            wrapperLength = totalWrapperLength;

            return packet;
        }

        protected virtual void ProcessAuthMethodSelection(Span<byte> buffer)
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

        protected virtual void ProcessCommandRequest(Span<byte> buffer)
        {
            buffer = Unwrap(buffer, out _, out _);
            var request = new CommandRequest(buffer.ToArray());
            request.Validate();

            var errorReply = new CommandResponse(new IPEndPoint(0, 0))
            {
                Version = ProxyConsts.Version,
            };

            // Check whether the client requested a valid command.
            if (Server.OfferedCommands.Contains(request.Command))
            {
                // FIXME: Some commands use this field as the client endpoint, not the destination.
                if (IsDestinationValid(request.ProxyEndpoint))
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
            Span<byte> bufferSpan = buffer.AsSpan((int)offset, (int)size);

            // Choose the authentication method.
            if (Auth == null)
            {
                ProcessAuthMethodSelection(bufferSpan);
                // TODO: We should avoid having special cases. Is this fine?
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

            bufferSpan = Unwrap(bufferSpan, out _, out _);

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
                buffer = Command.Wrap(buffer, null, out _);
            }

            if (IsAuthenticated)
            {
                buffer = Auth.Wrap(buffer, null, out _);
            }

            if (Command is { UsesDatagrams: true })
            {
                throw new InvalidOperationException($"{nameof(SendAsync)} can't be used when sending datagrams.");
            }

            if (Command is { HandlesCommunication: true })
            {
                throw new NotImplementedException("Async Send/Receive for commands has not been implemented yet.");
            }

            return base.SendAsync(buffer);
        }

        public override long Send(ReadOnlySpan<byte> buffer)
        {
            if (Command != null)
            {
                buffer = Command.Wrap(buffer, null, out _);
            }

            if (IsAuthenticated)
            {
                buffer = Auth.Wrap(buffer, null, out _);
            }

            if (Command is { UsesDatagrams: true })
            {
                throw new InvalidOperationException($"{nameof(Send)} can't be used when sending datagrams.");
            }

            if (Command is { HandlesCommunication: true })
            {
                return Command.Send(buffer);
            }

            return base.Send(buffer);
        }

        public int SendTo(ReadOnlySpan<byte> buffer, ProxyEndpoint endpoint)
        {
            if (Command == null)
            {
                throw new InvalidOperationException("No command was requested yet.");
            }

            if (!Command.UsesDatagrams)
            {
                throw new InvalidOperationException("The requested command is not able to send datagrams.");
            }

            buffer = Command.Wrap(buffer, endpoint, out int totalWrapperLength);

            if (IsAuthenticated)
            {
                buffer = Auth.Wrap(buffer, endpoint, out int wrapperLength);
                totalWrapperLength += wrapperLength;
            }

            return Command.SendTo(buffer, endpoint.ToEndPoint()) - totalWrapperLength;
        }

        public int SendTo(ReadOnlySpan<byte> buffer, EndPoint endpoint)
        {
            ProxyEndpoint remoteEndpoint = endpoint switch
            {
                IPEndPoint ipEndpoint => new ProxyEndpoint(ipEndpoint),
                DnsEndPoint dnsEndpoint => new ProxyEndpoint(dnsEndpoint),
                _ => throw new ArgumentException($"The provided type {endpoint} is not supported.", nameof(endpoint)),
            };

            return SendTo(buffer, remoteEndpoint);
        }
    }
}
