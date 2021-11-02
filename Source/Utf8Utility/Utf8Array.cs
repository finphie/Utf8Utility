using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Toolkit.HighPerformance;
#if NET6_0_OR_GREATER
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;
using Utf8Utility.Helpers;
#endif

namespace Utf8Utility;

/// <summary>
/// UTF-8配列を表す構造体です。
/// </summary>
[SuppressMessage("Design", "CA1036:比較可能な型でメソッドをオーバーライドします", Justification = "配列")]
public readonly struct Utf8Array : IEquatable<Utf8Array>,
#if NET6_0_OR_GREATER
    ISpanFormattable, IComparable<Utf8Array>
#else
    IFormattable
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

#if NET6_0_OR_GREATER
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
        => Utf8.ToUtf16(_value, destination, out _, out charsWritten) == OperationStatus.Done;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Utf8Array other)
    {
        var xStart = DangerousGetReference();
        var yStart = other.DangerousGetReference();
        var index = 0;
        var length = Math.Min(Length, other.Length);

        while (index < length)
        {
            var xByteCount = UnicodeUtility.GetUtf8SequenceLength(xStart);
            var yByteCount = UnicodeUtility.GetUtf8SequenceLength(yStart);
            var diffUtf8SequenceLength = xByteCount - yByteCount;

            // 最初の要素が異なるバイト数の文字だった場合、バイト数が短い順にする。
            if (diffUtf8SequenceLength != 0)
            {
                return diffUtf8SequenceLength;
            }

            // 非Ascii文字の場合、バイナリを比較する。
            // xByteCount == yByteCountなのでyByteCountでの条件分岐は不要。
            if (xByteCount != 1)
            {
                return Utf16Compare(_value.AsSpan(index), other.AsSpan(index));
            }

            var xValue = (char)xStart;
            var yValue = (char)yStart;

            var xSpanValue = MemoryMarshal.CreateReadOnlySpan(ref xValue, 1);
            var ySpanValue = MemoryMarshal.CreateReadOnlySpan(ref yValue, 1);
            var result = xSpanValue.CompareTo(ySpanValue, StringComparison.InvariantCulture);

            if (result != 0)
            {
                return result;
            }

            // Ascii文字なのでオフセットに1を加算
            xStart = Unsafe.Add(ref xStart, 1);
            yStart = Unsafe.Add(ref yStart, 1);
            index++;
        }

        return Length - other.Length;

        static int Utf16Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
        {
            var xCount = Encoding.UTF8.GetCharCount(x);
            Span<char> xBuffer = stackalloc char[xCount];

            if (Utf8.ToUtf16(x, xBuffer, out _, out _) != OperationStatus.Done)
            {
                goto Error;
            }

            var yCount = Encoding.UTF8.GetCharCount(y);
            Span<char> yBuffer = stackalloc char[yCount];

            if (Utf8.ToUtf16(y, yBuffer, out _, out _) != OperationStatus.Done)
            {
                goto Error;
            }

            return ((ReadOnlySpan<char>)xBuffer).CompareTo(yBuffer, StringComparison.InvariantCulture);

        Error:
            throw new ArgumentException();
        }
    }
#endif

    /// <summary>
    /// <see cref="ReadOnlySpan{Byte}"/>構造体を取得します。
    /// </summary>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>構造体</returns>
    public ReadOnlySpan<byte> AsSpan() => _value;

    /// <summary>
    /// <see cref="ReadOnlySpan{Byte}"/>構造体を取得します。
    /// </summary>
    /// <param name="start">初期インデックス</param>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>構造体</returns>
    public ReadOnlySpan<byte> AsSpan(int start) =>
#if NET6_0_OR_GREATER
        AsSpan()[start..];
#else
        AsSpan().Slice(start);
#endif

    /// <summary>
    /// 最初の要素への参照を取得します。
    /// このメソッドは境界チェックを行いません。
    /// </summary>
    /// <returns>最初の要素への参照</returns>
    public ref byte DangerousGetReference() => ref _value.DangerousGetReference();

    /// <summary>
    /// 指定された要素への参照を取得します。
    /// このメソッドは境界チェックを行いません。
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <returns>指定された要素への参照</returns>
    public ref byte DangerousGetReferenceAt(int index) => ref _value.DangerousGetReferenceAt(index);
}
