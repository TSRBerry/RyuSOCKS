using System;

namespace RyuSocks.Commands
{
    public interface ICommand
    {
        /// <summary>
        /// Command ID used in SOCKS requests
        /// </summary>
        public static byte Id => throw new NotImplementedException();
    }
}
