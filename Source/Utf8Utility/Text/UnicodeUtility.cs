using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Utf8Utility.Helpers;

#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using CommunityToolkit.HighPerformance;
#endif

namespace Utf8Utility.Text;

/// <summary>
/// Unicode関連のユーティリティクラスです。
/// </summary>
public static partial class UnicodeUtility
{
    /// <summary>
    /// 指定されたUTF-8文字のバイト数を取得します。
    /// </summary>
    /// <param name="value">値</param>
    /// <returns>指定されたUTF-8文字のバイト数を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetUtf8SequenceLength(byte value)
    {
        ReadOnlySpan<byte> trailingBytesForUTF8 = new byte[]
        {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
            4, 4, 4, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        ref var table = ref MemoryMarshal.GetReference(trailingBytesForUTF8);
        return Unsafe.Add(ref table, (nint)value);
    }

    /// <summary>
    /// 指定された値がAscii文字かどうかを判定します。
    /// </summary>
    /// <param name="value">値</param>
    /// <returns>
    /// Ascii文字の場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiCodePoint(byte value) => value <= 0x7Fu;

    /// <summary>
    /// UTF-8文字数を取得します。
    /// </summary>
    /// <param name="value">UTF-8文字列</param>
    /// <returns>UTF-8文字数を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetLength(ReadOnlySpan<byte> value)
    {
        var count = 0;

        ref var start = ref MemoryMarshal.GetReference(value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)value.Length);

#if NET7_0_OR_GREATER
        // 下記のURLを参考に最適化
        // https://qiita.com/saka1_p/items/ff49d981cfd56f3588cc
        // https://qiita.com/umezawatakeshi/items/ed23935788756c800b86
        if (Avx2.IsSupported && value.Length >= Vector256<byte>.Count)
        {
            nuint i = 0;
            var length32 = (nuint)(value.Length & -Vector256<byte>.Count);

            do
            {
                var sum = Vector256<byte>.Zero;
                var limit = Math.Min((nuint)(255 * Vector256<byte>.Count), length32 - i);
                nuint j = 0;

                for (; j < limit; j += (nuint)Vector256<byte>.Count)
                {
                    var vector = Vector256.LoadUnsafe(ref start, j);
                    sum = Avx2.Subtract(sum, Avx2.CompareGreaterThan(vector.AsSByte(), Vector256.Create<sbyte>(-0x41)).AsByte());
                }

                var sumHigh = Avx2.UnpackHigh(sum, Vector256<byte>.Zero);
                var sumLow = Avx2.UnpackLow(sum, Vector256<byte>.Zero);
                var sum16x16 = Avx2.Add(sumHigh.AsInt16(), sumLow.AsInt16());
                var sum16x8 = Avx2.Add(sum16x16, Avx2.Permute2x128(sum16x16, sum16x16, 1));

                const byte Control = (0 << 6) | (0 << 4) | (2 << 2) | 3;
                var sum16x4 = Avx2.Add(sum16x8, Avx2.Shuffle(sum16x8.AsInt32(), Control).AsInt16());

                var temp = sum16x4.AsInt64().GetElement(0);
                count += (int)((temp >> 0) & 0xFFFF);
                count += (int)((temp >> 16) & 0xFFFF);
                count += (int)((temp >> 32) & 0xFFFF);
                count += (int)((temp >> 48) & 0xFFFF);

                i += j;
            }
            while (i < length32);

            start = ref Unsafe.AddByteOffset(ref start, i);
        }
#endif

#if NET7_0_OR_GREATER
        if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong))
#else
        if ((nint)Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong))
#endif
        {
            end = ref Unsafe.SubtractByteOffset(ref end, sizeof(ulong));

            do
            {
                var number = Unsafe.ReadUnaligned<ulong>(ref start);

                const ulong Mask = 0x8080808080808080 >> 7;
                var x = ((number >> 6) | (~number >> 7)) & Mask;

                count += BitOperations.PopCount(x);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

            end = ref Unsafe.AddByteOffset(ref end, sizeof(ulong));
        }

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            if ((start & 0xC0) != 0x80)
            {
                count++;
            }

            start = ref Unsafe.AddByteOffset(ref start, 1);
        }

        return count;
    }

#if NET7_0_OR_GREATER
    /// <summary>
    /// UTF-8文字列が空または空白かどうかを判定します。
    /// </summary>
    /// <param name="value">UTF-8文字列</param>
    /// <returns>
    /// UTF-8文字列が空または空白の場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmptyOrWhiteSpace(ReadOnlySpan<byte> value)
    {
        // Utf8Spanを参考に実装
        // https://github.com/dotnet/runtimelab/blob/84564a0e033114a1b2316c7bfb9953e4e3255cd3/src/libraries/System.Private.CoreLib/src/System/Text/Utf8Span.cs#L124
        // https://github.com/dotnet/runtimelab/blob/84564a0e033114a1b2316c7bfb9953e4e3255cd3/src/libraries/System.Private.CoreLib/src/System/Text/Unicode/Utf8Utility.WhiteSpace.CoreLib.cs#L11-L68
        nuint index = 0;
        var length = value.Length;

        while ((int)index < length)
        {
            ref var valueStart = ref Unsafe.AddByteOffset(ref value.DangerousGetReference(), index);

            // 文字コードが[0x21..0x7F]の範囲にあるか。
            if ((sbyte)valueStart > (sbyte)' ')
            {
                break;
            }

            // Ascii文字の場合のみ、処理を最適化する。
            // 空白確認には、{Rune|char}.IsWhiteSpaceを利用できる。
            // Rune.DecodeFromUtf8は引数チェックなどがあるので遅く、
            // 回避するためにUnsafe.Asでvalueを直接書き換える必要があるが、Ascii文字限定。
            // charにキャストして比較する方法もAscii文字限定。
            // したがって、最適化を行う場合はAscii文字かどうかでの分岐は必須。
            if (IsAsciiCodePoint(valueStart))
            {
                // 直前の処理でAscii文字であることは確定しているため、
                // {Rune|char}.IsWhiteSpaceを使用せず、自前実装で比較を行う。
                // 上記メソッドではAscii文字かどうかで判定が入ってしまうため。
                // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/Text/Rune.cs#L1350-L1366
                // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/Char.cs#L274-L287
                if (AsciiUtility.IsWhiteSpace(valueStart))
                {
                    index++;
                    continue;
                }
            }
            else
            {
                var span = MemoryMarshal.CreateReadOnlySpan(ref valueStart, length - (int)index);
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

        return (int)index == length;
    }

    /// <summary>
    /// 比較を行います。
    /// </summary>
    /// <param name="x">基準となるUTF-8文字列</param>
    /// <param name="y">比較対象のUTF-8文字列</param>
    /// <param name="comparisonType">比較規則</param>
    /// <returns>
    /// <paramref name="x"/>が<paramref name="y"/>より小さいか等しいか大きいかを判断して、
    /// 負の整数か0または正の整数を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        // UTF-16文字列に変換する際、最大4バイト必要となる。
        // stackalloc charでは遅いため、nintで定義する。
        Unsafe.SkipInit(out nint xBuffer);
        Unsafe.SkipInit(out nint yBuffer);

        ref var xBufferStart = ref Unsafe.As<nint, char>(ref xBuffer);
        ref var yBufferStart = ref Unsafe.As<nint, char>(ref yBuffer);

        ref var xStart = ref MemoryMarshal.GetReference(x);
        ref var yStart = ref MemoryMarshal.GetReference(y);

        ref var xEnd = ref Unsafe.AddByteOffset(ref xStart, (nint)(uint)x.Length);
        ref var yEnd = ref Unsafe.AddByteOffset(ref yStart, (nint)(uint)y.Length);

        do
        {
            WriteUtf16Span(ref xStart, out var xBytesConsumed, ref xBufferStart, out var charsWritten);
            var xSpan = MemoryMarshal.CreateReadOnlySpan(ref xBufferStart, charsWritten);

            WriteUtf16Span(ref yStart, out var yBytesConsumed, ref yBufferStart, out charsWritten);
            var ySpan = MemoryMarshal.CreateReadOnlySpan(ref yBufferStart, charsWritten);

            var result = xSpan.CompareTo(ySpan, comparisonType);

            if (result != 0)
            {
                return result;
            }

            xStart = ref Unsafe.AddByteOffset(ref xStart, (nint)(uint)xBytesConsumed);
            yStart = ref Unsafe.AddByteOffset(ref yStart, (nint)(uint)yBytesConsumed);
        }
        while (Unsafe.IsAddressLessThan(ref xStart, ref xEnd) && Unsafe.IsAddressLessThan(ref yStart, ref yEnd));

        // 到達条件
        // 1. 比較対象の片方または両方のUTF-8文字列が空。
        // 2. 途中まで文字が一致しており、片方または両方のUTF-8文字列の末尾に到達。
        // したがって、バイト数の差を比較すれば良い。
        return x.Length - y.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void WriteUtf16Span(ref byte valueStart, out int bytesConsumed, ref char destination, out int charsWritten)
        {
            // Ascii文字の場合のみ、処理を最適化する。
            if (IsAsciiCodePoint(valueStart))
            {
                destination = (char)valueStart;
                bytesConsumed = 1;
                charsWritten = 1;
                return;
            }

            // UTF-8文字を1文字だけ取得する。
            var length = GetUtf8SequenceLength(valueStart);
            var span = MemoryMarshal.CreateReadOnlySpan(ref valueStart, length);
            Rune.DecodeFromUtf8(span, out var rune, out bytesConsumed);

            // UTF-16にエンコードできないことはないはず。
            // https://github.com/dotnet/runtime/blob/v7.0.0-rc.1.22426.10/src/libraries/System.Private.CoreLib/src/System/Text/Rune.cs#L992-L1034
            rune.TryEncodeToUtf16(MemoryMarshal.CreateSpan(ref destination, 2), out charsWritten);
        }
    }
#endif
}
