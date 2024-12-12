// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 * The MIT License (MIT)
 *
 * Copyright (c) .NET Foundation and Contributors
 *
 * All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RyuSocks
{
    // Provides the underlying stream of data for network access.
    public class SocksClientStream : Stream
    {
        // Used by the class to hold the underlying SocksClient the stream uses.
        private readonly SocksClient _socksClient;

        // Whether the stream should dispose of the socket when the stream is disposed
        private readonly bool _ownsSocket;

        // Used by the class to indicate that the stream is m_Readable.
        private bool _readable;

        // Used by the class to indicate that the stream is writable.
        private bool _writeable;

        // Whether Dispose has been called.
        private bool _disposed;

        // Creates a new instance of the System.Net.Sockets.SocksClientStream class for the specified System.Net.Sockets.Socket.
        public SocksClientStream(SocksClient socket)
            : this(socket, FileAccess.ReadWrite, ownsSocket: false)
        {
        }

        public SocksClientStream(SocksClient socket, bool ownsSocket)
            : this(socket, FileAccess.ReadWrite, ownsSocket)
        {
        }

        public SocksClientStream(SocksClient socket, FileAccess access)
            : this(socket, access, ownsSocket: false)
        {
        }

        public SocksClientStream(SocksClient socket, FileAccess access, bool ownsSocket)
        {
            ArgumentNullException.ThrowIfNull(socket);

            if (!socket.Blocking)
            {
                // Stream.Read*/Write* are incompatible with the semantics of non-blocking sockets, and
                // allowing non-blocking sockets could result in non-deterministic failures from those
                // operations. A developer that requires using SocksClientStream with a non-blocking socket can
                // temporarily flip Socket.Blocking as a workaround.
                throw new IOException("Socket is not blocking.");
            }
            if (!socket.Connected)
            {
                throw new IOException("Socket is not connected.");
            }
            if (socket.SocketType != SocketType.Stream)
            {
                throw new IOException("SocketType is not Stream.");
            }

            _socksClient = socket;
            _ownsSocket = ownsSocket;

            switch (access)
            {
                case FileAccess.Read:
                    _readable = true;
                    break;
                case FileAccess.Write:
                    _writeable = true;
                    break;
                case FileAccess.ReadWrite:
                default: // assume FileAccess.ReadWrite
                    _readable = true;
                    _writeable = true;
                    break;
            }

            _socksClient.WaitForCommand(true);
        }

        public SocksClient SocksClient => _socksClient;

        // Used by the class to indicate that the stream is m_Readable.
        protected bool Readable
        {
            get { return _readable; }
            set { _readable = value; }
        }

        // Used by the class to indicate that the stream is writable.
        protected bool Writeable
        {
            get { return _writeable; }
            set { _writeable = value; }
        }

        // Indicates that data can be read from the stream.
        // We return the readability of this stream. This is a read only property.
        public override bool CanRead => _readable;

        // Indicates that the stream can seek a specific location
        // in the stream. This property always returns false.
        public override bool CanSeek => false;

        // Indicates that data can be written to the stream.
        public override bool CanWrite => _writeable;

        // Indicates whether we can timeout
        public override bool CanTimeout => true;

        // Set/Get ReadTimeout, note of a strange behavior, 0 timeout == infinite for sockets,
        // so we map this to -1, and if you set 0, we cannot support it
        public override int ReadTimeout
        {
            get
            {
                int timeout = (int)_socksClient.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
                if (timeout == 0)
                {
                    return -1;
                }
                return timeout;
            }
            set
            {
                if (value <= 0 && value != Timeout.Infinite)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Timeout must be greater than 0.");
                }
                SetSocketTimeoutOption(SocketShutdown.Receive, value);
            }
        }

        // Set/Get WriteTimeout, note of a strange behavior, 0 timeout == infinite for sockets,
        // so we map this to -1, and if you set 0, we cannot support it
        public override int WriteTimeout
        {
            get
            {
                int timeout = (int)_socksClient.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
                if (timeout == 0)
                {
                    return -1;
                }
                return timeout;
            }
            set
            {
                if (value <= 0 && value != Timeout.Infinite)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Timeout must be greater than 0.");
                }
                SetSocketTimeoutOption(SocketShutdown.Send, value);
            }
        }

        // Indicates data is available on the stream to be read.
        // This property checks to see if at least one byte of data is currently available
        public virtual bool DataAvailable
        {
            get
            {
                ThrowIfDisposed();

                // Ask the socket how many bytes are available. If it's
                // not zero, return true.
                return _socksClient.Available != 0;
            }
        }

        // The length of data available on the stream. Always throws NotSupportedException.
        public override long Length
        {
            get
            {
                throw new NotSupportedException("Seeking is not supported.");
            }
        }

        // Gets or sets the position in the stream. Always throws NotSupportedException.
        public override long Position
        {
            get
            {
                throw new NotSupportedException("Seeking is not supported.");
            }

            set
            {
                throw new NotSupportedException("Seeking is not supported.");
            }
        }

        // Seeks a specific position in the stream. This method is not supported by the
        // SocksClientStream class.
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seeking is not supported.");
        }

        // Read - provide core Read functionality.
        //
        // Provide core read functionality. All we do is call through to the
        // socket Receive functionality.
        //
        // Input:
        //
        //     Buffer  - Buffer to read into.
        //     Offset  - Offset into the buffer where we're to read.
        //     Count   - Number of bytes to read.
        //
        // Returns:
        //
        //     Number of bytes we read, or 0 if the socket is closed.
        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);
            ThrowIfDisposed();
            if (!CanRead)
            {
                throw new InvalidOperationException("Write-only stream");
            }

            int receivedSize = _socksClient.Receive(buffer.AsSpan(offset, count), SocketFlags.None, out SocketError error);

            if (error != SocketError.Success)
            {
                throw WrapException(new SocketException((int)error));
            }

            return receivedSize;
        }

        public override int Read(Span<byte> buffer)
        {
            if (GetType() != typeof(SocksClientStream))
            {
                // SocksClientStream is not sealed, and a derived type may have overridden Read(byte[], int, int) prior
                // to this Read(Span<byte>) overload being introduced.  In that case, this Read(Span<byte>) overload
                // should use the behavior of Read(byte[],int,int) overload.
                return base.Read(buffer);
            }

            ThrowIfDisposed();
            if (!CanRead)
            {
                throw new InvalidOperationException("Write-only stream");
            }

            int receivedSize = _socksClient.Receive(buffer, SocketFlags.None, out SocketError error);

            if (error != SocketError.Success)
            {
                throw WrapException(new SocketException((int)error));
            }

            return receivedSize;
        }

        public override unsafe int ReadByte()
        {
            byte b;
            return Read(new Span<byte>(&b, 1)) == 0 ? -1 : b;
        }

        // Write - provide core Write functionality.
        //
        // Provide core write functionality. All we do is call through to the
        // socket Send method..
        //
        // Input:
        //
        //     Buffer  - Buffer to write from.
        //     Offset  - Offset into the buffer from where we'll start writing.
        //     Count   - Number of bytes to write.
        //
        // Returns:
        //
        //     Number of bytes written. We'll throw an exception if we
        //     can't write everything. It's brutal, but there's no other
        //     way to indicate an error.
        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);
            ThrowIfDisposed();
            if (!CanWrite)
            {
                throw new InvalidOperationException("Read-only stream");
            }

            // Since the socket is in blocking mode this will always complete
            // after ALL the requested number of bytes was transferred.
            _socksClient.Send(buffer.AsSpan(offset, count), SocketFlags.None, out SocketError error);

            if (error != SocketError.Success)
            {
                throw WrapException(new SocketException((int)error));
            }
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (GetType() != typeof(SocksClientStream))
            {
                // SocksClientStream is not sealed, and a derived type may have overridden Write(byte[], int, int) prior
                // to this Write(ReadOnlySpan<byte>) overload being introduced.  In that case, this Write(ReadOnlySpan<byte>)
                // overload should use the behavior of Write(byte[],int,int) overload.
                base.Write(buffer);
                return;
            }

            ThrowIfDisposed();
            if (!CanWrite)
            {
                throw new InvalidOperationException("Read-only stream");
            }

            _socksClient.Send(buffer, SocketFlags.None, out SocketError error);

            if (error != SocketError.Success)
            {
                throw WrapException(new SocketException((int)error));
            }
        }

        public override unsafe void WriteByte(byte value) =>
            Write(new ReadOnlySpan<byte>(&value, 1));

        private int _closeTimeout; // -1 = respect linger options

        /// <summary>Closes the <see cref="SocksClientStream"/> after waiting the specified time to allow data to be sent.</summary>
        /// <param name="timeout">The number of milliseconds to wait to send any remaining data before closing.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is less than -1.</exception>
        /// <remarks>
        /// The Close method frees both unmanaged and managed resources associated with the <see cref="SocksClientStream"/>.
        /// If the <see cref="SocksClientStream"/> owns the underlying <see cref="SocksClient"/>, it is closed as well.
        /// If a <see cref="SocksClientStream"/> was associated with a <see cref="TcpClient"/>, the <see cref="Close(int)"/> method
        /// will close the TCP connection, but not dispose of the associated <see cref="TcpClient"/>.
        /// </remarks>
        public void Close(int timeout)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(timeout, -1);
            _closeTimeout = timeout;
            Dispose();
        }

        /// <summary>Closes the <see cref="SocksClientStream"/> after waiting the specified time to allow data to be sent.</summary>
        /// <param name="timeout">The amount of time to wait to send any remaining data before closing.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is less than -1 milliseconds or greater than <see cref="int.MaxValue"/> milliseconds.</exception>
        /// <remarks>
        /// The Close method frees both unmanaged and managed resources associated with the <see cref="SocksClientStream"/>.
        /// If the <see cref="SocksClientStream"/> owns the underlying <see cref="SocksClient"/>, it is closed as well.
        /// If a <see cref="SocksClientStream"/> was associated with a <see cref="TcpClient"/>, the <see cref="Close(int)"/> method
        /// will close the TCP connection, but not dispose of the associated <see cref="TcpClient"/>.
        /// </remarks>
        public void Close(TimeSpan timeout) => Close(ToTimeoutMilliseconds(timeout));

        private static int ToTimeoutMilliseconds(TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;

            ArgumentOutOfRangeException.ThrowIfLessThan(totalMilliseconds, -1, nameof(timeout));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(totalMilliseconds, int.MaxValue, nameof(timeout));

            return (int)totalMilliseconds;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                // The only resource we need to free is the network stream, since this
                // is based on the client socket, closing the stream will cause us
                // to flush the data to the network, close the stream and (in the
                // NetoworkStream code) close the socket as well.
                _readable = false;
                _writeable = false;
                if (_ownsSocket)
                {
                    // If we own the Socket (false by default), close it
                    // ignoring possible exceptions (eg: the user told us
                    // that we own the Socket but it closed at some point of time,
                    // here we would get an ObjectDisposedException)
                    try
                    {
                        _socksClient.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                        // ignored
                    }

                    _socksClient.Close(_closeTimeout);
                }
            }

            base.Dispose(disposing);
        }

        ~SocksClientStream() => Dispose(false);

        // BeginRead - provide async read functionality.
        //
        // This method provides async read functionality. All we do is
        // call through to the underlying socket async read.
        //
        // Input:
        //
        //     buffer  - Buffer to read into.
        //     offset  - Offset into the buffer where we're to read.
        //     size   - Number of bytes to read.
        //
        // Returns:
        //
        //     An IASyncResult, representing the read.
        // public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        // {
        //     ValidateBufferArguments(buffer, offset, count);
        //     ThrowIfDisposed();
        //     if (!CanRead)
        //     {
        //         throw new InvalidOperationException(SR.net_writeonlystream);
        //     }
        //
        //     try
        //     {
        //         return _socksClient.BeginReceive(
        //                 buffer,
        //                 offset,
        //                 count,
        //                 SocketFlags.None,
        //                 callback,
        //                 state);
        //     }
        //     catch (Exception exception) when (!(exception is OutOfMemoryException))
        //     {
        //         throw WrapException(SR.net_io_readfailure, exception);
        //     }
        // }

        // EndRead - handle the end of an async read.
        //
        // This method is called when an async read is completed. All we
        // do is call through to the core socket EndReceive functionality.
        //
        // Returns:
        //
        //     The number of bytes read. May throw an exception.
        // public override int EndRead(IAsyncResult asyncResult)
        // {
        //     ThrowIfDisposed();
        //     ArgumentNullException.ThrowIfNull(asyncResult);
        //
        //     try
        //     {
        //         return _socksClient.EndReceive(asyncResult);
        //     }
        //     catch (Exception exception) when (!(exception is OutOfMemoryException))
        //     {
        //         throw WrapException(SR.net_io_readfailure, exception);
        //     }
        // }

        // BeginWrite - provide async write functionality.
        //
        // This method provides async write functionality. All we do is
        // call through to the underlying socket async send.
        //
        // Input:
        //
        //     buffer  - Buffer to write into.
        //     offset  - Offset into the buffer where we're to write.
        //     size   - Number of bytes to written.
        //
        // Returns:
        //
        //     An IASyncResult, representing the write.
        // public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        // {
        //     ValidateBufferArguments(buffer, offset, count);
        //     ThrowIfDisposed();
        //     if (!CanWrite)
        //     {
        //         throw new InvalidOperationException(SR.net_readonlystream);
        //     }
        //
        //     try
        //     {
        //         // Call BeginSend on the Socket.
        //         return _socksClient.BeginSend(
        //                 buffer,
        //                 offset,
        //                 count,
        //                 SocketFlags.None,
        //                 callback,
        //                 state);
        //     }
        //     catch (Exception exception) when (!(exception is OutOfMemoryException))
        //     {
        //         throw WrapException(SR.net_io_writefailure, exception);
        //     }
        // }

        // Handle the end of an asynchronous write.
        // This method is called when an async write is completed. All we
        // do is call through to the core socket EndSend functionality.
        // Returns:  The number of bytes read. May throw an exception.
        // public override void EndWrite(IAsyncResult asyncResult)
        // {
        //     ThrowIfDisposed();
        //     ArgumentNullException.ThrowIfNull(asyncResult);
        //
        //     try
        //     {
        //         _socksClient.EndSend(asyncResult);
        //     }
        //     catch (Exception exception) when (!(exception is OutOfMemoryException))
        //     {
        //         throw WrapException(SR.net_io_writefailure, exception);
        //     }
        // }

        // ReadAsync - provide async read functionality.
        //
        // This method provides async read functionality. All we do is
        // call through to the Begin/EndRead methods.
        //
        // Input:
        //
        //     buffer            - Buffer to read into.
        //     offset            - Offset into the buffer where we're to read.
        //     size              - Number of bytes to read.
        //     cancellationToken - Token used to request cancellation of the operation
        //
        // Returns:
        //
        //     A Task<int> representing the read.
        // public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        // {
        //     ValidateBufferArguments(buffer, offset, count);
        //     ThrowIfDisposed();
        //     if (!CanRead)
        //     {
        //         throw new InvalidOperationException(SR.net_writeonlystream);
        //     }
        //
        //     try
        //     {
        //         return _socksClient.ReceiveAsync(
        //             new Memory<byte>(buffer, offset, count),
        //             SocketFlags.None,
        //             fromSocksClientStream: true,
        //             cancellationToken).AsTask();
        //     }
        //     catch (Exception exception) when (!(exception is OutOfMemoryException))
        //     {
        //         throw WrapException(SR.net_io_readfailure, exception);
        //     }
        // }
        //
        // public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        // {
        //     bool canRead = CanRead; // Prevent race with Dispose.
        //     ThrowIfDisposed();
        //     if (!canRead)
        //     {
        //         throw new InvalidOperationException(SR.net_writeonlystream);
        //     }
        //
        //     try
        //     {
        //         return _socksClient.ReceiveAsync(
        //             buffer,
        //             SocketFlags.None,
        //             fromSocksClientStream: true,
        //             cancellationToken: cancellationToken);
        //     }
        //     catch (Exception exception) when (!(exception is OutOfMemoryException))
        //     {
        //         throw WrapException(SR.net_io_readfailure, exception);
        //     }
        // }

        // WriteAsync - provide async write functionality.
        //
        // This method provides async write functionality. All we do is
        // call through to the Begin/EndWrite methods.
        //
        // Input:
        //
        //     buffer  - Buffer to write into.
        //     offset  - Offset into the buffer where we're to write.
        //     size    - Number of bytes to write.
        //     cancellationToken - Token used to request cancellation of the operation
        //
        // Returns:
        //
        //     A Task representing the write.
        // public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        // {
        //     ValidateBufferArguments(buffer, offset, count);
        //     ThrowIfDisposed();
        //     if (!CanWrite)
        //     {
        //         throw new InvalidOperationException(SR.net_readonlystream);
        //     }
        //
        //     try
        //     {
        //         return _socksClient.SendAsyncForSocksClientStream(
        //             new ReadOnlyMemory<byte>(buffer, offset, count),
        //             SocketFlags.None,
        //             cancellationToken).AsTask();
        //     }
        //     catch (Exception exception) when (!(exception is OutOfMemoryException))
        //     {
        //         throw WrapException(SR.net_io_writefailure, exception);
        //     }
        // }
        //
        // public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        // {
        //     bool canWrite = CanWrite; // Prevent race with Dispose.
        //     ThrowIfDisposed();
        //     if (!canWrite)
        //     {
        //         throw new InvalidOperationException(SR.net_readonlystream);
        //     }
        //
        //     try
        //     {
        //         return _socksClient.SendAsyncForSocksClientStream(
        //             buffer,
        //             SocketFlags.None,
        //             cancellationToken);
        //     }
        //     catch (Exception exception) when (!(exception is OutOfMemoryException))
        //     {
        //         throw WrapException(SR.net_io_writefailure, exception);
        //     }
        // }

        // Flushes data from the stream.  This is meaningless for us, so it does nothing.
        public override void Flush()
        {
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        // Sets the length of the stream. Always throws NotSupportedException
        public override void SetLength(long value)
        {
            throw new NotSupportedException("Seeking is not supported.");
        }

        private int _currentReadTimeout = -1;
        private int _currentWriteTimeout = -1;
        internal void SetSocketTimeoutOption(SocketShutdown mode, int timeout)
        {
            if (timeout < 0)
            {
                timeout = 0; // -1 becomes 0 for the winsock stack
            }

            if (mode == SocketShutdown.Send || mode == SocketShutdown.Both)
            {
                if (timeout != _currentWriteTimeout)
                {
                    _socksClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
                    _currentWriteTimeout = timeout;
                }
            }

            if (mode == SocketShutdown.Receive || mode == SocketShutdown.Both)
            {
                if (timeout != _currentReadTimeout)
                {
                    _socksClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
                    _currentReadTimeout = timeout;
                }
            }
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        private static IOException WrapException(SocketException innerException)
        {
            return new IOException(innerException.Message, innerException);
        }
    }
}
