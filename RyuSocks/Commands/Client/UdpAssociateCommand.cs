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

namespace RyuSocks.Commands.Client
{
    [ProxyCommandImpl(0x03)]
    public partial class UdpAssociateCommand : ClientCommand
    {
        public UdpAssociateCommand(SocksClient client, ProxyEndpoint source) : base(client, source)
        {
            CommandRequest request = new(source)
            {
                Version = ProxyConsts.Version,
                Command = ProxyCommand.UdpAssociate,
            };

            request.Validate();
            Client.SendAsync(request.AsSpan());
        }
    }
}
