// Copyright (C) RyuSOCKS
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2,
// as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using RyuSocks.Packets;
using RyuSocks.Types;
using RyuSocks.Utils;
using System;
using System.Net;

namespace RyuSocks.Commands.Server
{
    [ProxyCommandImpl(0x03)]
    public partial class UdpAssociateCommand : ServerCommand, IDisposable
    {
        public override bool HandlesCommunication => true;
        public override bool UsesDatagrams => true;
        private UdpServer _server;

        public UdpAssociateCommand(SocksSession session, IPEndPoint boundEndpoint, ProxyEndpoint source) : base(session, boundEndpoint, source)
        {
            _server = new UdpServer(this, boundEndpoint, source);

            _server.Start();
        }

        public override void OnReceived(ReadOnlySpan<byte> buffer)
        {
            if (!_server.IsStarted)
            {
                throw new InvalidOperationException("Server isn't running.");
            }

            buffer = RemoveUdpHeader(buffer, out IPEndPoint endpoint);
            _server.SendAsync(endpoint, buffer);
        }

        private static ReadOnlySpan<byte> AddUdpHeader(IPEndPoint endpoint, ReadOnlySpan<byte> buffer)
        {
            UdpPacket packet = new(endpoint, buffer.Length);
            buffer.CopyTo(packet.UserData);
            packet.Validate();

            return packet.AsSpan();
        }

        private static ReadOnlySpan<byte> RemoveUdpHeader(ReadOnlySpan<byte> buffer, out IPEndPoint endpoint)
        {
            UdpPacket packet = new(buffer.ToArray());
            packet.Validate();

            endpoint = new IPEndPoint(packet.DestinationAddress, packet.DestinationPort);

            return packet.UserData;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_server != null)
                {
                    _server.Stop();
                    _server.Dispose();
                    _server = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        class UdpServer : NetCoreServer.UdpServer
        {
            private readonly UdpAssociateCommand _command;
            private readonly ProxyEndpoint _sourceEndpoint;

            public UdpServer(UdpAssociateCommand command, IPEndPoint endpoint, ProxyEndpoint source) : base(endpoint)
            {
                _command = command;
                _sourceEndpoint = source;
            }

            protected override void OnStarted()
            {
                CommandResponse response = Endpoint switch
                {
                    IPEndPoint ipEndPoint => new CommandResponse(ipEndPoint)
                    {
                        Version = ProxyConsts.Version,
                        ReplyField = ReplyField.Succeeded,
                    },
                    DnsEndPoint dnsEndPoint => new CommandResponse(dnsEndPoint)
                    {
                        Version = ProxyConsts.Version,
                        ReplyField = ReplyField.Succeeded,
                    },
                    _ => throw new InvalidOperationException(
                        $"The type of EndPoint is not supported: {Endpoint}"),
                };

                _command.Session.SendAsync(response.AsSpan());
            }

            protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
            {
                if ((!Equals(_sourceEndpoint, ProxyEndpoint.Null) && !_sourceEndpoint.Contains((IPEndPoint)endpoint)))
                {
                    return;
                }

                _command.Session.SendAsync(AddUdpHeader((IPEndPoint)endpoint, buffer.AsSpan((int)offset, (int)size)));
            }
        }
    }
}
