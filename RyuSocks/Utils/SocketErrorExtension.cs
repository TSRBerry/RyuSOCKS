using System.Net.Sockets;

namespace RyuSocks.Utils
{
    public static class SocketErrorExtension
    {
        public static ReplyField ToReplyField(this SocketError error)
        {
            return error switch
            {
                SocketError.Success => ReplyField.Succeeded,
                SocketError.ConnectionRefused => ReplyField.ConnectionRefused,
                SocketError.HostUnreachable => ReplyField.HostUnreachable,
                SocketError.NetworkUnreachable => ReplyField.NetworkUnreachable,
                _ => ReplyField.ServerFailure,
            };
        }
    }
}
