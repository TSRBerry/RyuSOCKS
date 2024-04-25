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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace RyuSocks.Types
{
    public class ProxyEndpoint
    {
        public static ProxyEndpoint Null => new(new IPEndPoint(0, 0));

        public AddressType Type { get; private init; }
        public ushort Port { get; private init; }
        public IReadOnlySet<IPAddress> Addresses { get; private init; }
        public string DomainName { get; private init; }

        public ProxyEndpoint(IPEndPoint endpoint)
        {
            Type = endpoint.AddressFamily switch
            {
                AddressFamily.InterNetwork => AddressType.Ipv4Address,
                AddressFamily.InterNetworkV6 => AddressType.Ipv6Address,
                _ => throw new ArgumentException($"Unsupported AddressFamily: {endpoint.AddressFamily}", nameof(endpoint)),
            };

            Addresses = new HashSet<IPAddress> { endpoint.Address };
            Port = (ushort)endpoint.Port;
        }

        public ProxyEndpoint(DnsEndPoint endpoint)
        {
            if (endpoint.Host.Length is 0 or > 255)
            {
                throw new ArgumentException("Length of Host must be between 1 and 255.", nameof(endpoint));
            }

            Type = AddressType.DomainName;
            DomainName = endpoint.Host;
            Addresses = new HashSet<IPAddress>(Dns.GetHostAddresses(endpoint.Host));
            Port = (ushort)endpoint.Port;
        }

        public EndPoint ToEndPoint()
        {
            return Type switch
            {
                AddressType.Ipv4Address or AddressType.Ipv6Address => new IPEndPoint(Addresses.First(), Port),
                AddressType.DomainName => new DnsEndPoint(DomainName, Port),
                _ => throw new InvalidOperationException($"{nameof(ProxyEndpoint)} is not initialized."),
            };
        }

        public bool Contains(IPEndPoint endpoint)
        {
            return endpoint.Port == Port && Addresses.Contains(endpoint.Address);
        }

        public override string ToString()
        {
            return ToEndPoint().ToString();
        }
    }
}
