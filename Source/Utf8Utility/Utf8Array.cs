using System.Text;
using Microsoft.Toolkit.HighPerformance;

namespace Utf8Utility;

/// <summary>
/// UTF-8配列を表す構造体です。
/// </summary>
public readonly struct Utf8Array
#if NET6_0_OR_GREATER
    : IEquatable<Utf8Array>, ISpanFormattable
#else
    : IEquatable<Utf8Array>, IFormattable
#endif
{
    readonly byte[] _value;

    /// <summary>
    /// <see cref="Utf8Array"/>構造体の新しいインスタンスを取得します。
    /// </summary>
    /// <param name="bytes">UTF-8でエンコードされた<see cref="byte"/>配列</param>
    public Utf8Array(byte[] bytes) => _value = bytes;

    /// <summary>
    /// <see cref="Utf8Array"/>構造体の新しいインスタンスを取得します。
    /// </summary>
    /// <param name="s">文字列</param>
    public Utf8Array(string s) => _value = Encoding.UTF8.GetBytes(s);

    /// <summary>
    /// <see cref="Utf8Array"/>構造体の新しいインスタンスを取得します。
    /// </summary>
    /// <param name="bytes">UTF-8でエンコードされた<see cref="ReadOnlySpan{T}"/>構造体</param>
    public Utf8Array(ReadOnlySpan<byte> bytes) => _value = bytes.ToArray();

    /// <summary>
    /// <see cref="Utf8Array"/>構造体の新しいインスタンスを取得します。
    /// </summary>
    /// <param name="chars">UTF-16でエンコードされた<see cref="ReadOnlySpan{T}"/>構造体</param>
    public Utf8Array(ReadOnlySpan<char> chars)
    {
        int count;

#if NET5_0_OR_GREATER
        count = Encoding.UTF8.GetByteCount(chars);
        _value = new byte[count];
        Encoding.UTF8.GetBytes(chars, _value);
#else
        if (chars.IsEmpty)
        {
            _value = Array.Empty<byte>();
            return;
        }

        unsafe
        {
            fixed (char* c = chars)
            {
                count = Encoding.UTF8.GetByteCount(c, chars.Length);
                _value = new byte[count];

                fixed (byte* bytes = _value)
                {
                    Encoding.UTF8.GetBytes(c, chars.Length, bytes, _value.Length);
                }
            }
        }
#endif
    }

    /// <summary>
    /// 空文字列を表す<see cref="Utf8Array"/>インスタンスを取得します。
    /// </summary>
    /// <value>
    /// 空文字列を表す<see cref="Utf8Array"/>インスタンス
    /// </value>
    public static Utf8Array Empty { get; }

    /// <summary>
    /// バイト数を取得します。
    /// </summary>
    /// <value>
    /// バイト数
    /// </value>
    public int Length => _value.Length;

    /// <summary>
    /// <see cref="Utf8Array"/>構造体の新しいインスタンスを取得します。
    /// </summary>
    /// <param name="bytes">UTF-8でエンコードされた<see cref="byte"/>配列</param>
    public static explicit operator Utf8Array(byte[] bytes) => new(bytes);

    /// <summary>
    /// <see cref="Utf8Array"/>構造体の新しいインスタンスを取得します。
    /// </summary>
    /// <param name="s">文字列</param>
    public static explicit operator Utf8Array(string s) => new(s);

    /// <summary>
    /// 指定されたインスタンスのオブジェクトが等しいかどうかを表します。
    /// </summary>
    /// <param name="left"><see cref="Utf8Array"/>インスタンス</param>
    /// <param name="right">比較対象の><see cref="Utf8Array"/>インスタンス</param>
    /// <returns>
    /// 同じ文字列を表す場合は<see langword="true"/>、
    /// 異なる場合は<see langword="false"/>。
    /// </returns>
    public static bool operator ==(Utf8Array left, Utf8Array right) => left.Equals(right);

    /// <summary>
    /// 指定されたインスタンスのオブジェクトが異なるかどうかを表します。
    /// </summary>
    /// <param name="left"><see cref="Utf8Array"/>インスタンス</param>
    /// <param name="right">比較対象の><see cref="Utf8Array"/>インスタンス</param>
    /// <returns>
    /// 異なる文字列を表す場合は<see langword="true"/>、
    /// 同じ場合は<see langword="false"/>。
    /// </returns>
    public static bool operator !=(Utf8Array left, Utf8Array right) => !(left == right);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is Utf8Array x && Equals(x);

    /// <inheritdoc/>
    public bool Equals(Utf8Array other)
        => _value.AsSpan().SequenceEqual(other._value);

    /// <inheritdoc/>
    public override int GetHashCode() => _value.GetDjb2HashCode();

    /// <inheritdoc/>
    public override string ToString() => Encoding.UTF8.GetString(_value);

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

#if NET6_0_OR_GREATER
    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => System.Text.Unicode.Utf8.ToUtf16(_value, destination, out _, out charsWritten) == System.Buffers.OperationStatus.Done;
#endif

    /// <summary>
    /// <see cref="ReadOnlySpan{Byte}"/>構造体を取得します。
    /// </summary>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>構造体</returns>
    public ReadOnlySpan<byte> AsSpan() => _value;

    /// <summary>
    /// 指定されたインデックスの値を取得します。
    /// このメソッドは、境界チェックを行いません。
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <returns>指定されたインデックスの値</returns>
    public byte DangerousGetByte(int index) => _value.DangerousGetReferenceAt(index);
}
