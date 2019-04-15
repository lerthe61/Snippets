using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProducerConsumerStream
{
    /// <summary>
    /// Lazy Stream implementation that would use delegate to keep feeding stream while consumer request new portion of
    /// data. Not thread safe.
    /// </summary>
    ///<remarks>
    /// Performance may vary depending on <param name="desiredBufferedLength" />. No deep optimization was done.
    /// </remarks>
    public sealed class ProducerConsumerStream : Stream
    {
        internal const int DefaultBufferSize = 65536;

        private readonly Func<TextWriter, bool> _producer;
        private bool _finished = false;
        private long _length = 0L;
        private int _desiredBufferedLength = 0;
        private int _currentBufferedLength = 0;
        private int _bufferPos = 0;
        private readonly LinkedList<byte[]> _bufferList;
        private readonly CustomTextWriter _writer;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _length;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="producer">A delegate that should populate first parameter with new portion of data. And return false if there is no data left</param>
        /// <param name="desiredBufferedLength">Desired size of internal buffer. May have major impact on overall performance</param>
        public ProducerConsumerStream(Func<TextWriter, bool> producer, int desiredBufferedLength)
        {
            _bufferList = new LinkedList<byte[]>();
            _producer = producer;
            _desiredBufferedLength = desiredBufferedLength;
            _writer = new CustomTextWriter(_bufferList, _desiredBufferedLength);
        }

        // Only a couple of methods was overridden! In the case of using with data that do not string or char additional methods has to be overridden.
        private class CustomTextWriter : TextWriter
        {
            private int _desiredBufferedLength;
            private byte[] _buffer;
            private int _bufferPosition = 0;
            private readonly LinkedList<byte[]> _storage;
            public override Encoding Encoding { get; }

            public int DesiredBufferedLength
            {
                get => _desiredBufferedLength;
                set => _desiredBufferedLength = Math.Max(value, DefaultBufferSize);
            }

            public CustomTextWriter(LinkedList<byte[]> destination, int desiredBufferedLength)
            {
                _buffer = new byte[DefaultBufferSize];
                _storage = destination;
                _desiredBufferedLength = desiredBufferedLength;
                Encoding = Encoding.Default;
            }

            public override void Write(char value)
            {
                var bytes = Encoding.GetBytes(new[] {value});
                Write(bytes, 0, bytes.Length);
            }

            public override void Write(string value)
            {
                var bytes = Encoding.GetBytes(value);
                Write(bytes, 0, bytes.Length);
            }

            private void Write(byte[] buffer, int offset, int count)
            {
                var remainingAmount = count;
                var destinationOffset = offset;
                while (remainingAmount > 0)
                {
                    var copyAmount = Math.Min(remainingAmount, _buffer.Length - _bufferPosition);
                    Buffer.BlockCopy(buffer, destinationOffset, _buffer, _bufferPosition, copyAmount);

                    _bufferPosition += copyAmount;
                    remainingAmount -= copyAmount;
                    destinationOffset += copyAmount;

                    if (_bufferPosition == _buffer.Length)
                    {
                        _storage.AddLast(new LinkedListNode<byte[]>(_buffer));
                        _buffer = new byte[DesiredBufferedLength];
                        _bufferPosition = 0;
                    }
                }
            }

            public override void Close()
            {
                Flush();
                base.Close();
            }

            public override void Flush()
            {
                if (_bufferPosition == 0) return;

                // cut remaining part of the array
                Array.Resize(ref _buffer, _bufferPosition);

                _storage.AddLast(new LinkedListNode<byte[]>(_buffer));
                _buffer = new byte[_desiredBufferedLength];
                _bufferPosition = 0;
            }
        }

        /// <summary>
        /// Make internal buffer available to consumer. Usually there is no need to call this method, since <see cref="Read"/> is capable to do the same.
        /// </summary>
        public override void Flush()
        {
            _writer.Flush();
        }

        /// <summary>Reads a block of bytes from the current stream and writes the data to a buffer.</summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between offset and (offset + count - 1) replaced by the characters read from the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing data from the current stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes written into the buffer. This can be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached before any bytes are read.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer">buffer</paramref> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset">offset</paramref> or <paramref name="count">count</paramref> is negative.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="offset">offset</paramref> subtracted from the buffer length is less than <paramref name="count">count</paramref>.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The current stream instance is closed.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length - offset < count) throw new ArgumentException("Amount of requested data should be less or equal to available amount");

            if (count > _desiredBufferedLength) _desiredBufferedLength = count;
            _writer.DesiredBufferedLength = _desiredBufferedLength;

            if (_currentBufferedLength - _bufferPos < count)
            {
                FetchData();
            }
            
            return ReadBuffered(buffer, offset, count);
        }

        private int ReadBuffered(byte[] buffer, int offset, int count)
        {
            var currentPosition = offset;
            var remainingAmount = count;
            while (remainingAmount > 0)
            {
                var firstChunk = _bufferList.First;
                if (firstChunk == null)
                    return 0;

                var copyAmount = MathExtensions.Min(remainingAmount, buffer.Length - currentPosition, firstChunk.Value.Length - _bufferPos);
                Buffer.BlockCopy(firstChunk.Value, _bufferPos, buffer, currentPosition, copyAmount);

                _bufferPos += copyAmount;
                currentPosition += copyAmount;
                remainingAmount -= copyAmount;
                
                if (firstChunk.Value.Length == _bufferPos)
                {
                    _bufferPos = 0;
                    _bufferList.RemoveFirst();
                    _currentBufferedLength -= firstChunk.Value.Length;
                    if (_bufferList.Count == 0) break;
                }
            }

            return count - remainingAmount;
        }

        private void FetchData()
        {
            if (_finished) return;

            var previousBufferLength = _currentBufferedLength;
            while (_currentBufferedLength - _bufferPos <= _desiredBufferedLength)
            {
                var result = _producer(_writer);

                // If end of data
                if (!result)
                {
                    _finished = true;
                    _writer.Flush(); // forcing _writer to flush content, since it is end of stream and we do not expect more data
                    _currentBufferedLength = _bufferList.Sum(_ => _.Length); // In this case _bufferList would contain all content
                    break;
                }
                else
                {
                    // BufferList may not be populated at this point, but we would prefer to keep reading until internal buffer in _writer gets filled
                    _currentBufferedLength = _bufferList.Sum(_ => _.Length);
                }
            }

            _length += _currentBufferedLength - previousBufferLength;
        }

        #region NotSupported
        public override long Position
        {
            get =>
                throw new NotSupportedException(
                    $"Cannot get position on {nameof(ProducerConsumerStream)}");
            set =>
                throw new NotSupportedException(
                    $"Cannot set position on {nameof(ProducerConsumerStream)}");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException($"Cannot seek on {nameof(ProducerConsumerStream)}");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException($"Cannot set length on {nameof(ProducerConsumerStream)}");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException($"Cannot write on {nameof(ProducerConsumerStream)}");
        }
        #endregion
    }
}