using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Toolkit.HighPerformance;
using Utf8Utility.Helpers;
#if NET6_0_OR_GREATER
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Unicode;
using Utf8Utility.Text;
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
    public int ByteCount => _value.Length;

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
        var length = _value.Length - start;
        ref var valueStart = ref Unsafe.AddByteOffset(ref DangerousGetReference(), (nint)(uint)length);
        return MemoryMarshal.CreateReadOnlySpan(ref valueStart, length);
#else
        var span = new ReadOnlySpan<byte>(_value, start, _value.Length - start);
        return span;
#endif
    }

    /// <summary>
    /// UTF-8文字数を取得します。
    /// </summary>
    /// <returns>UTF-8文字数</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetLength()
    {
        var count = 0;
        nuint index = 0;

#if NET6_0_OR_GREATER
        const ulong Mask = 0x8080808080808080 >> 7;
        var length = ByteCount - sizeof(ulong);

        // 8バイト単位でカウントする。
        while ((int)index <= length)
        {
            // 最適化の関係でrefローカル変数にしてはいけない。
            var value = Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index));

            var x = ((value >> 6) | (~value >> 7)) & Mask;
            count += BitOperations.PopCount(x);
            index += sizeof(ulong);
        }
#endif

        // 1バイト単位でカウントする。
        while ((int)index < ByteCount)
        {
            // 最適化の関係でrefローカル変数にしてはいけない。
#if NET6_0_OR_GREATER
            var value = Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index);
#else
            var value = Unsafe.AddByteOffset(ref _value.DangerousGetReference(), (nint)index);
#endif

            if ((value & 0xC0) != 0x80)
            {
                count++;
            }

            index++;
        }

        return count;
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// UTF-8配列が空または空白かどうかを判定します。
    /// </summary>
    /// <returns>
    /// UTF-8配列が空または空白の場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmptyOrWhiteSpace()
    {
        // Utf8Spanを参考に実装
        // https://github.com/dotnet/runtimelab/blob/84564a0e033114a1b2316c7bfb9953e4e3255cd3/src/libraries/System.Private.CoreLib/src/System/Text/Utf8Span.cs#L124
        // https://github.com/dotnet/runtimelab/blob/84564a0e033114a1b2316c7bfb9953e4e3255cd3/src/libraries/System.Private.CoreLib/src/System/Text/Unicode/Utf8Utility.WhiteSpace.CoreLib.cs#L11-L68
        nuint index = 0;

        while ((int)index < _value.Length)
        {
            ref var valueStart = ref DangerousGetReference();
            ref var value = ref Unsafe.AddByteOffset(ref valueStart, index);

            // 文字コードが[0x21..0x7F]の範囲にあるか。
            if ((sbyte)value > (sbyte)' ')
            {
                break;
            }

            // Ascii文字の場合のみ、処理を最適化する。
            // 空白確認には、{Rune|char}.IsWhiteSpaceを利用できる。
            // Rune.DecodeFromUtf8は引数チェックなどがあるので遅く、
            // 回避するためにUnsafe.Asでvalueを直接書き換える必要があるが、Ascii文字限定。
            // charにキャストして比較する方法もAscii文字限定。
            // したがって、最適化を行う場合はAscii文字かどうかでの分岐は必須。
            if (UnicodeUtility.IsAsciiCodePoint(value))
            {
                // 直前の処理でAscii文字であることは確定しているため、
                // {Rune|char}.IsWhiteSpaceを使用せず、自前実装で比較を行う。
                // 上記メソッドではAscii文字かどうかで判定が入ってしまうため。
                // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/Text/Rune.cs#L1350-L1366
                // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/Char.cs#L274-L287
                if (AsciiUtility.IsWhiteSpace(value))
                {
                    index++;
                    continue;
                }
            }
            else
            {
                var span = MemoryMarshal.CreateReadOnlySpan(ref value, _value.Length - (int)index);
                Rune.DecodeFromUtf8(span, out var rune, out var bytesConsumed);

                if (Rune.IsWhiteSpace(rune))
                {
                    index += (uint)bytesConsumed;
                    continue;
                }
            }

            // ここに到達した場合、空白以外の文字のはず。
            break;
        }

        return (int)index == _value.Length;
    }
#endif

    /// <summary>
    /// UTF-8配列が、Ascii文字のみで構成されているかどうかを判定します。
    /// </summary>
    /// <returns>
    /// UTF-8配列が、Ascii文字のみで構成されている場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAscii() => UnicodeUtility.IsAscii(_value);

    /// <summary>
    /// 最初の要素への参照を取得します。
    /// このメソッドは境界チェックを行いません。
    /// </summary>
    /// <returns>最初の要素への参照</returns>
    public ref byte DangerousGetReference() => ref _value.DangerousGetReference();

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
