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

namespace RyuSocks.Commands.Client
{
    [ProxyCommandImpl(0x02)]
    public partial class BindCommand : ClientCommand
    {
        public BindCommand(SocksClient client, ProxyEndpoint source) : base(client, source)
        {
            CommandRequest request;

            switch (source.ToEndPoint())
            {
                case IPEndPoint ipSource:
                    request = new CommandRequest(ipSource)
                    {
                        Version = ProxyConsts.Version,
                        Command = ProxyCommand.Bind,
                    };
                    break;
                case DnsEndPoint dnsSource:
                    request = new CommandRequest(dnsSource)
                    {
                        Version = ProxyConsts.Version,
                        Command = ProxyCommand.Bind,
                    };
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(EndPoint)} provided.", nameof(source));
            }

            request.Validate();
            Client.SendAsync(request.AsSpan());
        }
    }
}
