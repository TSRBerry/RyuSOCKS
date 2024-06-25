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

using RyuSocks.Packets;
using RyuSocks.Types;
using RyuSocks.Utils;
using System;

namespace RyuSocks.Commands.Client
{
    [ProxyCommandImpl(0x01)]
    public partial class ConnectCommand : ClientCommand
    {
        public override bool HandlesCommunication => false;
        public override bool UsesDatagrams => false;

        public ConnectCommand(SocksClient client, ProxyEndpoint destination) : base(client, destination)
        {
            CommandRequest request = new(destination)
            {
                Version = ProxyConsts.Version,
                Command = ProxyCommand.Connect,
            };

            request.Validate();
            Client.Send(request.AsSpan());
        }

        public override void ProcessResponse(CommandResponse response)
        {
            EnsureSuccessReply(response.ReplyField);

            if (ServerEndpoint == null)
            {
                ServerEndpoint = response.ProxyEndpoint;
                Accepted = true;
                Ready = true;
                return;
            }

            throw new InvalidOperationException($"Unexpected invocation of {nameof(ProcessResponse)}. {nameof(ServerEndpoint)} is already assigned.");
        }
    }
}
