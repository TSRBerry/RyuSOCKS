using RyuSocks.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace RyuSocks
{
    public class TcpClient : NetCoreServer.TcpClient
    {
        public SocketError Error { get; private set; } = SocketError.Success;
        public event EventHandler<ReceivedEventArgs> Received;

        public TcpClient(IPAddress address, int port) : base(address, port)
        {
        }

        public TcpClient(string address, int port) : base(address, port)
        {
        }

        public TcpClient(DnsEndPoint endpoint) : base(endpoint)
        {
        }

        public TcpClient(IPEndPoint endpoint) : base(endpoint)
        {
        }

        public void ResetError()
        {
            Error = SocketError.Success;
        }

        public override bool Connect()
        {
            ResetError();
            bool result = base.Connect();

            if (!result && Error == SocketError.Success)
            {
                Error = SocketError.ConnectionRefused;
            }

            return result;
        }

        protected override void OnConnected()
        {
            ReceiveAsync();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            Received?.Invoke(this, new ReceivedEventArgs
            {
                Buffer = buffer,
                Offset = offset,
                Size = size,
            });

            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
        {
            Error = error;
        }
    }
}
