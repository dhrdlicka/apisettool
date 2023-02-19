namespace ApiSetTool.PortableExecutable;

public class PeSectionStream : Stream
{
    long _position;
    Stream _stream;

    PeSectionHeader _section;

    internal PeSectionStream(PeSectionHeader section)
    {
        _section = section;
        _stream = _section.Executable.GetFileStream();
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => Math.Min(_section.VirtualSize, _section.SizeOfRawData);

    public override long Position
    {
        get => _position;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(Position));

            if (value < Length)
                _stream.Position = _section.PointerToRawData + value;

            _position = value;
        }
    }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        count = (int)Math.Min(_position + count, Length - _position);
        if (count <= 0) return 0;

        Seek(0, SeekOrigin.Current);

        var bytesRead = 0;

        if (_position < _section.VirtualSize)
        {
            var sectionSize = Math.Min(_section.SizeOfRawData, _section.VirtualSize);
            bytesRead += _stream.Read(buffer, offset, (int)Math.Min(count, sectionSize));
        }

        for (var i = bytesRead; i < count; i++)
        {
            buffer[i] = 0;
        }

        return count;
    }

    public override long Seek(long offset, SeekOrigin origin)
        => Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => Length - offset,
            _ => throw new ArgumentOutOfRangeException(null, nameof(origin))
        };

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}
