using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Chimera.Core.Macro.History;

/// <summary>
/// Unmanaged 64-byte UTF-8 string buffer for historical chronicle ledgers.
/// Guarantees zero heap allocations during creation, storage, and comparison.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 64)]
public unsafe struct FixedString64 : IEquatable<FixedString64>
{
    public const int MaxCapacity = 63;

    private fixed byte _buffer[63];
    private byte _length;

    public int Length => _length;
    public bool IsEmpty => _length == 0;

    public FixedString64(ReadOnlySpan<char> text)
    {
        _length = 0;
        if (text.IsEmpty) return;

        fixed (byte* ptr = _buffer)
        {
            var span = new Span<byte>(ptr, MaxCapacity);
            int bytesWritten = Encoding.UTF8.GetBytes(text, span);
            _length = (byte)Math.Min(bytesWritten, MaxCapacity);
        }
    }

    public FixedString64(string? text) : this(text.AsSpan()) { }

    public ReadOnlySpan<byte> AsSpan()
    {
        fixed (byte* ptr = _buffer)
        {
            return new ReadOnlySpan<byte>(ptr, _length);
        }
    }

    public override string ToString()
    {
        if (_length == 0) return string.Empty;
        fixed (byte* ptr = _buffer)
        {
            return Encoding.UTF8.GetString(ptr, _length);
        }
    }

    public bool Equals(FixedString64 other)
    {
        if (_length != other._length) return false;
        fixed (byte* a = _buffer)
        {
            byte* b = other._buffer;
            return new ReadOnlySpan<byte>(a, _length).SequenceEqual(new ReadOnlySpan<byte>(b, _length));
        }
    }

    public override bool Equals(object? obj) => obj is FixedString64 other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        fixed (byte* ptr = _buffer)
        {
            for (int i = 0; i < _length; i++)
            {
                hash.Add(ptr[i]);
            }
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(FixedString64 left, FixedString64 right) => left.Equals(right);
    public static bool operator !=(FixedString64 left, FixedString64 right) => !left.Equals(right);

    public static implicit operator FixedString64(string text) => new(text);
}
