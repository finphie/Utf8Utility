using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Toolkit.HighPerformance;
#if NET6_0_OR_GREATER
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text.Unicode;
#else
using Utf8Utility.Helpers;
#endif

namespace Utf8Utility;

/// <summary>
/// UTF-8配列を表す構造体です。
/// </summary>
#if NET6_0_OR_GREATER
[SuppressMessage("Design", "CA1036:比較可能な型でメソッドをオーバーライドします", Justification = "配列")]
#endif
public readonly partial struct Utf8Array : IEquatable<Utf8Array>,
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
    /// <exception cref="ArgumentNullException">引数がnullの場合、この例外をスローします。</exception>
    public Utf8Array(byte[] bytes)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(bytes);
#else
        if (bytes is null)
        {
            ThrowHelper.ThrowArgumentNullException(nameof(bytes));
        }
#endif

        _value = bytes;
    }

    /// <summary>
    /// <see cref="Utf8Array"/>構造体の新しいインスタンスを取得します。
    /// </summary>
    /// <param name="s">文字列</param>
    /// <exception cref="ArgumentNullException">引数がnullの場合、この例外をスローします。</exception>
    public Utf8Array(string s)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(s);
#else
        if (s is null)
        {
            ThrowHelper.ThrowArgumentNullException(nameof(s));
        }
#endif

        _value = Encoding.UTF8.GetBytes(s);
    }

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
    public static Utf8Array Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => EmptyArray.Value;
    }

    /// <summary>
    /// バイト数を取得します。
    /// </summary>
    /// <value>
    /// バイト数
    /// </value>
    public int Length => _value.Length;

    /// <summary>
    /// UTF-8配列が空かどうかを判定します。
    /// </summary>
    /// <returns>
    /// UTF-8配列が空の場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>。
    /// </returns>
    [SuppressMessage("Style", "IDE0075:条件式を簡略化する", Justification = "最適化のため")]
    public bool IsEmpty
    {
        // インライン化された場合の最適化のため、三項演算子でtrue/falseを返す。
        // https://github.com/dotnet/runtime/issues/4207
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_value is null || _value.Length == 0) ? true : false;
    }

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
    public int CompareTo(Utf8Array other) => Compare(this, other);
#endif

    /// <summary>
    /// <see cref="ReadOnlySpan{Byte}"/>構造体を取得します。
    /// </summary>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>構造体</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> AsSpan()
    {
        // 引数の検証をスキップするために、手動でReadOnlySpanを作成する。
#if NET6_0_OR_GREATER
        ref var valueStart = ref DangerousGetReference();
        return MemoryMarshal.CreateReadOnlySpan(ref valueStart, _value.Length);
#else
        var span = new ReadOnlySpan<byte>(_value, 0, _value.Length);
        return span;
#endif
    }

    /// <summary>
    /// <see cref="ReadOnlySpan{Byte}"/>構造体を取得します。
    /// </summary>
    /// <param name="start">初期インデックス</param>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>構造体</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> AsSpan(int start)
    {
        // 引数の検証をスキップするために、手動でReadOnlySpanを作成する。
#if NET6_0_OR_GREATER
        ref var valueStart = ref DangerousGetReferenceAt(start);
        return MemoryMarshal.CreateReadOnlySpan(ref valueStart, _value.Length);
#else
        var span = new ReadOnlySpan<byte>(_value, start, _value.Length);
        return span;
#endif
    }

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

    /// <summary>
    /// <see cref="Utf8Array"/>構造体の新しいインスタンスを取得します。
    /// このメソッドは引数チェックを行いません。
    /// </summary>
    /// <param name="bytes">UTF-8でエンコードされた<see cref="byte"/>配列</param>
    /// <returns><see cref="Utf8Array"/>構造体の新しいインスタンス</returns>
    internal static Utf8Array UnsafeCreate(byte[] bytes)
    {
        Utf8Array array = default;
        Unsafe.AsRef(array._value) = bytes;

        return array;
    }

    static class EmptyArray
    {
        public static readonly Utf8Array Value = UnsafeCreate(Array.Empty<byte>());
    }
}
