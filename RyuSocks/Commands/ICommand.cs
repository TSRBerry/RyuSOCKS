using System;

namespace RyuSocks.Commands
{
    public interface ICommand
    {
        /// <summary>
        /// Command ID used in SOCKS requests
        /// </summary>
        public static ProxyCommand Id => throw new NotImplementedException();
    }
}
