using System;

namespace RyuSocks.Packets
{
    public abstract partial class Packet
    {
        /// <summary>
        /// The contents of the packet.
        /// </summary>
        public byte[] Bytes { get; protected set; }

        /// <inheritdoc cref="Bytes"/>
        public byte this[int i]
        {
            get => Bytes[i];
            set => Bytes[i] = value;
        }

        /// <summary>
        /// Creates a new span over the packet.
        /// </summary>
        /// <seealso cref="Bytes"/>
        /// <seealso cref="M:System.MemoryExtensions.AsSpan``1(``0[])"/>
        public Span<byte> AsSpan() => Bytes;

        /// <summary>
        /// Creates a new Span over the portion of the packet beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="start">The index at which to begin the Span.</param>
        /// <param name="length">The number of items in the Span.</param>
        /// <seealso cref="M:System.MemoryExtensions.AsSpan``1(``0[],System.Int32,System.Int32)"/>
        public Span<byte> AsSpan(int start, int length) => Bytes.AsSpan(start, length);

        protected Packet() { }

        protected Packet(byte[] bytes)
        {
            Bytes = bytes;
        }
    }
}
