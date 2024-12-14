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

using RyuSocks.Auth;
using RyuSocks.Commands;
using RyuSocks.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RyuSocks
{
    public partial class SocksServer : IDisposable
    {
        // TODO: Add (generated) properties for auth methods and commands
        private readonly Socket _socket;
        private readonly EndPoint _bindEndpoint;
        private SocketAsyncEventArgs _socketAcceptEvent;
        protected readonly List<SocksSession> Sessions = [];

        public IReadOnlySet<AuthMethod> AcceptableAuthMethods { get; set; } = new HashSet<AuthMethod>();
        public IReadOnlySet<ProxyCommand> OfferedCommands { get; set; } = new HashSet<ProxyCommand>();
        public bool UseAllowList { get; set; }
        public bool UseBlockList { get; set; }
        public IReadOnlyDictionary<IPAddress, ushort[]> AllowedDestinations { get; set; } = new Dictionary<IPAddress, ushort[]>();
        public IReadOnlyDictionary<IPAddress, ushort[]> BlockedDestinations { get; set; } = new Dictionary<IPAddress, ushort[]>();

        public EndPoint LocalEndPoint => _socket.LocalEndPoint;

        public SocksServer(IPAddress address, ushort port = ProxyConsts.DefaultPort) : this(new IPEndPoint(address, port)) { }

        public SocksServer(DnsEndPoint endpoint)
        {
            _socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _bindEndpoint = endpoint;
        }

        public SocksServer(IPEndPoint endpoint)
        {
            _socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _bindEndpoint = endpoint;
        }

        public void Start()
        {
            _socket.Bind(_bindEndpoint);
            _socket.Listen();
            AcceptNewSession();
        }

        private void AcceptNewSession()
        {
            _socketAcceptEvent?.Dispose();
            _socketAcceptEvent = new SocketAsyncEventArgs();
            _socketAcceptEvent.Completed += OnSessionAccepted;

            if (!_socket.AcceptAsync(_socketAcceptEvent))
            {
                OnSessionAccepted(this, _socketAcceptEvent);
            }
        }

        private void OnSessionAccepted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                // TODO: Log error
                return;
            }

            SocksSession session = new(this, e.AcceptSocket!);
            Sessions.Add(session);

            AcceptNewSession();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _socketAcceptEvent?.Dispose();
                _socketAcceptEvent = null;
                foreach (var session in Sessions)
                {
                    session?.Dispose();
                }
                Sessions.Clear();
                _socket?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
