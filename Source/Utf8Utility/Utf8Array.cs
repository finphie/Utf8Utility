using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Helpers;
using Utf8Utility.Text;

namespace Utf8Utility;

/// <summary>
/// UTF-8配列を表す構造体です。
/// </summary>
#if NET6_0_OR_GREATER
[SuppressMessage("Design", "CA1036:比較可能な型でメソッドをオーバーライドします", Justification = "配列")]
#endif
public readonly partial struct Utf8Array : IEquatable<Utf8Array>,
#if NET8_0_OR_GREATER
    IUtf8SpanFormattable,
#endif
#if NET6_0_OR_GREATER
    ISpanFormattable, IComparable<Utf8Array>
#else
    IFormattable
#endif
{
    readonly byte[] _value;

    [Obsolete("Do not use default constructor.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable
    public Utf8Array() => throw new NotSupportedException();
#pragma warning restore

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
        Guard.IsNotNull(bytes);
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
        Guard.IsNotNull(s);
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
        var count = Encoding.UTF8.GetByteCount(chars);
        _value = new byte[count];
        Encoding.UTF8.GetBytes(chars, _value);
    }

    /// <summary>
    /// 空文字列を表す<see cref="Utf8Array"/>インスタンスを取得します。
    /// </summary>
    /// <value>
    /// 空文字列を表す<see cref="Utf8Array"/>インスタンスを返します。
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
    /// 内部配列のバイト数を返します。
    /// </value>
    public int ByteCount => _value.Length;

    /// <summary>
    /// UTF-8配列が空かどうかを取得します。
    /// </summary>
    /// <value>
    /// UTF-8配列が空の場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>を返します。
    /// </value>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NET7_0_OR_GREATER

        get => _value is null || _value.Length == 0;
#else
        // インライン化された場合の最適化のため、三項演算子でtrue/falseを返す。
        // https://github.com/dotnet/runtime/issues/4207
        [SuppressMessage("Style", "IDE0075:条件式を簡略化する", Justification = "最適化のため")]
        get => (_value is null || _value.Length == 0) ? true : false;
#endif
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
    /// 異なる場合は<see langword="false"/>を返します。
    /// </returns>
    public static bool operator ==(Utf8Array left, Utf8Array right) => left.Equals(right);

    /// <summary>
    /// 指定されたインスタンスのオブジェクトが異なるかどうかを表します。
    /// </summary>
    /// <param name="left"><see cref="Utf8Array"/>インスタンス</param>
    /// <param name="right">比較対象の><see cref="Utf8Array"/>インスタンス</param>
    /// <returns>
    /// 異なる文字列を表す場合は<see langword="true"/>、
    /// 同じ場合は<see langword="false"/>を返します。
    /// </returns>
    public static bool operator !=(Utf8Array left, Utf8Array right) => !(left == right);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is Utf8Array x && Equals(x);

    /// <inheritdoc/>
    public bool Equals(Utf8Array other)
        => AsSpan().SequenceEqual(other.AsSpan());

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode<byte>.Combine(AsSpan());

    /// <inheritdoc/>
    public override string ToString() => Encoding.UTF8.GetString(_value);

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

#if NET8_0_OR_GREATER
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (!TryCopyTo(utf8Destination))
        {
            bytesWritten = 0;
            return false;
        }

        bytesWritten = _value.Length;
        return true;
    }
#endif

#if NET6_0_OR_GREATER
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => TryGetChars(destination, out charsWritten);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Utf8Array other) => Compare(this, other);
#endif

    /// <summary>
    /// UTF-8配列を指定された出力先にコピーします。
    /// </summary>
    /// <param name="destination">出力先</param>
    /// <returns>コピーに成功した場合は<see langword="true"/>を返します。失敗した場合は<see langword="false"/>を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryCopyTo(Span<byte> destination)
    {
        if (ByteCount > destination.Length)
        {
            return false;
        }

        CopyToInternal(destination);
        return true;
    }

    /// <summary>
    /// UTF-8配列を指定された出力先にコピーします。
    /// </summary>
    /// <param name="destination">出力先</param>
    /// <exception cref="ArgumentException">コピー先のサイズが不足している場合、この例外をスローします。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<byte> destination)
    {
        Guard.HasSizeGreaterThanOrEqualTo(destination, ByteCount);
        CopyToInternal(destination);
    }

    /// <summary>
    /// <see cref="ReadOnlySpan{Byte}"/>構造体を取得します。
    /// このメソッドは引数チェックを行いません。
    /// </summary>
    /// <param name="start">初期インデックス</param>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>構造体を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> DangerousAsSpan(int start)
    {
        // 引数の検証をスキップするために、手動でReadOnlySpanを作成する。
        var length = _value.Length - start;
        ref var valueStart = ref Unsafe.AddByteOffset(ref DangerousGetReference(), (nint)(uint)start);
        return MemoryMarshal.CreateReadOnlySpan(ref valueStart, length);
    }

    /// <summary>
    /// <see cref="ReadOnlySpan{Byte}"/>構造体を取得します。
    /// </summary>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>構造体を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> AsSpan()
    {
        // 引数の検証をスキップするために、手動でReadOnlySpanを作成する。
        ref var valueStart = ref DangerousGetReference();
        return MemoryMarshal.CreateReadOnlySpan(ref valueStart, _value.Length);
    }

    /// <summary>
    /// <see cref="ReadOnlySpan{Byte}"/>構造体を取得します。
    /// </summary>
    /// <param name="start">初期インデックス</param>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>構造体を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> AsSpan(int start) => new(_value, start, _value.Length - start);

    /// <summary>
    /// UTF-8でエンコードされた<see cref="byte"/>配列を取得します。
    /// 内部の配列をそのまま返すため、書き換えは行わないでください。
    /// </summary>
    /// <returns>UTF-8でエンコードされた<see cref="byte"/>配列を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] DangerousAsByteArray() => _value;

    /// <summary>
    /// UTF-8文字数を取得します。
    /// </summary>
    /// <returns>UTF-8文字数を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetLength() => UnicodeUtility.GetLength(AsSpan());

    /// <summary>
    /// UTF-8配列をUTF-16配列に変換します。
    /// </summary>
    /// <param name="destination">出力先</param>
    /// <param name="charsWritten">UTF-16配列の長さ</param>
    /// <returns>変換に成功した場合は<see langword="true"/>を返します。失敗した場合は<see langword="false"/>を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetChars(Span<char> destination, out int charsWritten)
    {
        var span = AsSpan();

#if NET8_0_OR_GREATER
        return Encoding.UTF8.TryGetChars(span, destination, out charsWritten);
#else
        var required = Encoding.UTF8.GetCharCount(span);

        if (required > destination.Length)
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = Encoding.UTF8.GetChars(span, destination);
        return true;
#endif
    }

    /// <summary>
    /// UTF-8配列をUTF-16配列に変換します。
    /// </summary>
    /// <param name="destination">出力先</param>
    /// <returns>UTF-16配列の長さを返します。</returns>
    /// <exception cref="ArgumentException">コピー先のサイズが不足している場合、この例外をスローします。</exception>
    public int GetChars(Span<char> destination) => Encoding.UTF8.GetChars(AsSpan(), destination);

#if NET6_0_OR_GREATER
    /// <summary>
    /// UTF-8配列が空または空白かどうかを判定します。
    /// </summary>
    /// <returns>
    /// UTF-8配列が空または空白の場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmptyOrWhiteSpace() => UnicodeUtility.IsEmptyOrWhiteSpace(AsSpan());
#endif

    /// <summary>
    /// UTF-8配列が、Ascii文字のみで構成されているかどうかを判定します。
    /// </summary>
    /// <returns>
    /// UTF-8配列が、Ascii文字のみで構成されている場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAscii() => UnicodeUtility.IsAscii(AsSpan());

    /// <summary>
    /// 最初の要素への参照を取得します。
    /// このメソッドは境界チェックを行いません。
    /// </summary>
    /// <returns>最初の要素への参照を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref byte DangerousGetReference() => ref _value.DangerousGetReference();

    /// <summary>
    /// <see cref="Utf8Array"/>構造体の新しいインスタンスを取得します。
    /// このメソッドは引数チェックを行いません。
    /// </summary>
    /// <param name="bytes">UTF-8でエンコードされた<see cref="byte"/>配列</param>
    /// <returns><see cref="Utf8Array"/>構造体の新しいインスタンスを返します。</returns>
    internal static Utf8Array UnsafeCreate(byte[] bytes)
    {
        Utf8Array array = default;
        Unsafe.AsRef(array._value) = bytes;

        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void CopyToInternal(Span<byte> destination)
        => Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(destination), ref DangerousGetReference(), (uint)_value.Length);

    static class EmptyArray
    {
        public static readonly Utf8Array Value = UnsafeCreate(Array.Empty<byte>());
    }
}
