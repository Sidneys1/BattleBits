using System;
using System.IO;

namespace Core
{
    public class CircularStream : Stream
    {
        private readonly Stream _innerStream;
        public CircularStream(Stream stream) {
            _innerStream = stream;
        }

        public override int Read(Span<byte> buffer) {
            int toRead = buffer.Length;

            var readBufPos = 0;
            while (toRead > 0) {
                int leftInStream = (int)(_innerStream.Length - _innerStream.Position);
                if (leftInStream <= toRead) {
                    _innerStream.Read(buffer.Slice(readBufPos, leftInStream));
                    _innerStream.Position = 0;
                    toRead -= leftInStream;
                } else {
                    _innerStream.Read(buffer.Slice(readBufPos));
                    break;
                }
            }

            return buffer.Length;
        }

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override long Position {
            get => _innerStream.Position;
            set => _innerStream.Position = value % _innerStream.Length;
        }

        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanSeek => true;

        public override long Length => _innerStream.Length;

        public override void SetLength(long value) => _innerStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public override void Flush() => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                case SeekOrigin.End:
                    return _innerStream.Seek(offset % _innerStream.Length, origin);
                case SeekOrigin.Current:
                    var pos = offset + _innerStream.Position;
                    return _innerStream.Seek(pos % _innerStream.Length, SeekOrigin.Begin);
            }
            return -1;
        }
    }
}
