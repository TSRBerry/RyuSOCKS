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
    [SocksClass]
    public partial class SocksSession : TcpSession
    {
        private bool _isClosing;
        private bool _isAuthenticated;
        private AuthMethod _authMethod = AuthMethod.NoAcceptableMethods;
        private ProxyCommand _command = 0;
        private IProxyAuth _auth;
        private ServerCommand _server;

        private void ProcessAuthMethodSelection(byte[] buffer)
        {
            var request = new MethodSelectionRequest(buffer);
            request.Validate();

            foreach (var requestedAuthMethod in request.Methods)
            {
                if (((SocksServer)this.Server).AcceptableAuthMethods.Contains(requestedAuthMethod))
                {
                    var reply = new MethodSelectionResponse(requestedAuthMethod)
                    {
                        Version = ProxyConsts.Version,
                    };

                    SendAsync(reply.Bytes);
                    _auth = requestedAuthMethod.GetAuth();
                    _authMethod = requestedAuthMethod;

                    return;
                }
            }

            var errorReply = new MethodSelectionResponse(AuthMethod.NoAcceptableMethods)
            {
                Version = ProxyConsts.Version,
            };

            SendAsync(errorReply.Bytes);
            _isClosing = true;
        }

        private bool IsDestinationValid(CommandRequest request)
        {
            if (!((SocksServer)Server).UseAllowList && !((SocksServer)Server).UseBlockList)
            {
                return true;
            }

            bool isDestinationValid = false;

            // Check whether the client is allowed to connect to the requested destination.
            foreach (IPAddress destinationAddress in request.Destination.Addresses)
            {
                if (((SocksServer)Server).UseAllowList
                    && ((SocksServer)Server).AllowedDestinations.TryGetValue(destinationAddress, out ushort[] allowedPorts)
                    && allowedPorts.Contains(request.DestinationPort))
                {
                    isDestinationValid = true;
                    break;
                }

                if (((SocksServer)Server).UseBlockList
                    && (!((SocksServer)Server).BlockedDestinations.TryGetValue(destinationAddress, out allowedPorts)
                        || !allowedPorts.Contains(request.DestinationPort)))
                {
                    isDestinationValid = true;
                    break;
                }
            }

            return isDestinationValid;
        }

        private void ProcessCommandRequest(byte[] buffer)
        {
            var request = new CommandRequest(buffer);
            request.Validate();

            var errorReply = new CommandResponse(new IPEndPoint(0, 0))
            {
                Version = ProxyConsts.Version,
            };

            // Check whether the client requested a valid command.
            if (((SocksServer)this.Server).OfferedCommands.Contains(request.Command))
            {
                if (IsDestinationValid(request))
                {
                    _server = request.Command.GetServerCommand()(this, (IPEndPoint)Server.Endpoint, request.Destination);
                    _command = request.Command;

                    return;
                }

                errorReply.ReplyField = ReplyField.ConnectionNotAllowed;
                SendAsync(errorReply.Bytes);
                _isClosing = true;

                return;
            }

            errorReply.ReplyField = ReplyField.CommandNotSupported;
            SendAsync(errorReply.Bytes);
            _isClosing = true;
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            // Choose the authentication method.
            if (_authMethod == AuthMethod.NoAcceptableMethods)
            {
                ProcessAuthMethodSelection(buffer[(int)offset..(int)size]);

                _isAuthenticated = _authMethod == AuthMethod.NoAuth;

                return;
            }

            // Authenticate the client.
            if (!_isAuthenticated)
            {
                try
                {
                    _isAuthenticated = _auth.Authenticate(buffer.AsSpan((int)offset, (int)size), out ReadOnlySpan<byte> sendBuffer);
                    SendAsync(sendBuffer);
                }
                catch (AuthenticationException)
                {
                    // TODO: Log the exception here.
                    _isClosing = true;
                }

                return;
            }

            // Attempt to process a command request.
            if (_command == 0)
            {
                ProcessCommandRequest(buffer[(int)offset..(int)size]);
                return;
            }

            // Don't process packets for clients we are disconnecting soon.
            if (_isClosing)
            {
                return;
            }

            var bufferSpan = _auth.Unwrap(buffer.AsSpan((int)offset, (int)size));
            bufferSpan = _server.Unwrap(bufferSpan);

            _server.OnReceived(bufferSpan);
        }

        protected override void OnEmpty()
        {
            if (_isClosing)
            {
                Disconnect();
            }
        }

        public override bool SendAsync(ReadOnlySpan<byte> buffer)
        {
            if (_command != 0)
            {
                buffer = _server.Wrap(buffer);
            }

            if (_isAuthenticated)
            {
                buffer = _auth.Wrap(buffer);
            }

            return base.SendAsync(buffer);
        }
    }
}
