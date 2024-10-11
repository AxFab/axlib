// This file is part of AxLib.
// 
// AxLib is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software 
// Foundation, either version 3 of the License, or (at your option) any later 
// version.
// 
// AxLib is distributed in the hope that it will be useful, but WITHOUT ANY 
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
// details.
// 
// You should have received a copy of the GNU General Public License along 
// with AxLib. If not, see <https://www.gnu.org/licenses/>.
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
namespace AxToolkit.Network;

public class TeeStream : Stream
{
    private Stream? _stream;
    private Stream _input;
    private Stream _output;
    private bool _read;
    private bool _write;

    public TeeStream(Stream stream, string input, string output)
    {
        _stream = stream;
        _read = _stream.CanRead;
        _write = _stream.CanWrite;
        _input = File.Open(input, FileMode.Create, FileAccess.Write);
        _output = File.Open(output, FileMode.Create, FileAccess.Write);
    }


    public override bool CanRead => _read;
    public override bool CanSeek => false;
    public override bool CanWrite => _write;
    public override long Length => throw new InvalidOperationException();

    public override long Position {
        get => throw new InvalidOperationException();
        set => throw new InvalidOperationException();
    }

    public override void Flush()
    {
        if (_stream == null)
            throw new ObjectDisposedException(nameof(_stream));
        _stream.Flush();
    }
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_stream == null)
            throw new ObjectDisposedException(nameof(_stream));
        if (!_read)
            throw new InvalidOperationException();
        var ret = _stream.Read(buffer, offset, count);
        _input.Write(buffer, offset, ret);
        _input.Flush();
        return ret;
    }

    public override long Seek(long offset, SeekOrigin origin)
        => throw new InvalidOperationException();

    public override void SetLength(long value)
        => throw new InvalidOperationException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_stream == null)
            throw new ObjectDisposedException(nameof(_stream));
        if (!_write)
            throw new InvalidOperationException();
        _stream.Write(buffer, offset, count);
        _output.Write(buffer, offset, count);
        _output.Flush();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _stream != null)
        {
            _stream.Dispose();
            _stream = null;
            _input.Dispose();
            _output.Dispose();
        }
    }
}
