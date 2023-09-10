using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

namespace Utf8Utility.Text;

/// <content>
/// <see cref="UnicodeUtility"/>クラスのIsAscii関連処理です。
/// </content>
partial class UnicodeUtility
{
    /// <summary>
    /// 指定された配列が、Ascii文字のみで構成されているかどうかを判定します。
    /// </summary>
    /// <param name="value">配列</param>
    /// <returns>
    /// 指定された配列が、Ascii文字のみで構成されている場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAscii(ReadOnlySpan<byte> value)
    {
        if (value.IsEmpty)
        {
            return false;
        }

        ref var start = ref MemoryMarshal.GetReference(value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)value.Length);

#if NET7_0_OR_GREATER
        if (Vector256.IsHardwareAccelerated)
        {
            var mask1 = Vector256<byte>.Zero;
            var mask2 = Vector256<byte>.Zero;

            if (value.Length >= Vector256<byte>.Count * 2)
            {
                end = ref Unsafe.SubtractByteOffset(ref end, Vector256<byte>.Count * 2);

                do
                {
                    mask1 |= Vector256.LoadUnsafe(ref start);
                    mask2 |= Vector256.LoadUnsafe(ref start, (nuint)Vector256<byte>.Count);
                    start = ref Unsafe.AddByteOffset(ref start, Vector256<byte>.Count * 2);
                }
                while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

                mask1 |= mask2;
                end = ref Unsafe.AddByteOffset(ref end, Vector256<byte>.Count * 2);
            }

            if (Unsafe.ByteOffset(ref start, ref end) >= Vector256<byte>.Count)
            {
                mask1 |= Vector256.LoadUnsafe(ref start);
                start = ref Unsafe.AddByteOffset(ref start, Vector256<byte>.Count);
            }

            var mask3 = 0UL;

            if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong) * 2)
            {
                mask3 = Unsafe.ReadUnaligned<ulong>(ref start);
                mask3 |= Unsafe.ReadUnaligned<ulong>(ref Unsafe.AddByteOffset(ref start, sizeof(ulong)));

                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong) * 2);
            }

            if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong))
            {
                mask3 |= Unsafe.ReadUnaligned<ulong>(ref start);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }

            if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(uint))
            {
                mask3 |= Unsafe.ReadUnaligned<uint>(ref start);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(uint));
            }

            if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ushort))
            {
                mask3 |= Unsafe.ReadUnaligned<ushort>(ref start);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ushort));
            }

            if (Unsafe.IsAddressLessThan(ref start, ref end))
            {
                mask3 |= start;
            }

            return (mask1.ExtractMostSignificantBits() | (mask3 & 0x8080808080808080)) == 0;
        }

        if (Vector128.IsHardwareAccelerated)
        {
            var mask1 = Vector128<byte>.Zero;
            var mask2 = Vector128<byte>.Zero;

            if (value.Length >= Vector128<byte>.Count * 2)
            {
                end = ref Unsafe.SubtractByteOffset(ref end, Vector128<byte>.Count * 2);

                do
                {
                    mask1 |= Vector128.LoadUnsafe(ref start);
                    mask2 |= Vector128.LoadUnsafe(ref start, (nuint)Vector128<byte>.Count);
                    start = ref Unsafe.AddByteOffset(ref start, Vector128<byte>.Count * 2);
                }
                while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

                mask1 |= mask2;
                end = ref Unsafe.AddByteOffset(ref end, Vector128<byte>.Count * 2);
            }

            if (Unsafe.ByteOffset(ref start, ref end) >= Vector128<byte>.Count)
            {
                mask1 |= Vector128.LoadUnsafe(ref start);
                start = ref Unsafe.AddByteOffset(ref start, Vector128<byte>.Count);
            }

            var mask3 = 0UL;

            if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong))
            {
                mask3 |= Unsafe.ReadUnaligned<ulong>(ref start);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }

            if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(uint))
            {
                mask3 |= Unsafe.ReadUnaligned<uint>(ref start);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(uint));
            }

            if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ushort))
            {
                mask3 |= Unsafe.ReadUnaligned<ushort>(ref start);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ushort));
            }

            if (Unsafe.IsAddressLessThan(ref start, ref end))
            {
                mask3 |= start;
            }

            return (mask1.ExtractMostSignificantBits() | (mask3 & 0x8080808080808080)) == 0;
        }
#endif

        return SoftwareFallback(ref start, ref end, value.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool SoftwareFallback(ref byte start, ref byte end, int length)
        {
            var mask1 = 0UL;
            var mask2 = 0UL;
            var mask3 = 0UL;
            var mask4 = 0UL;

            if (length >= sizeof(ulong) * 4)
            {
                end = ref Unsafe.SubtractByteOffset(ref end, sizeof(ulong) * 4);

                do
                {
                    ref var current = ref Unsafe.As<byte, ulong>(ref start);

                    mask1 |= current;
                    mask2 |= Unsafe.Add(ref current, 1);
                    mask3 |= Unsafe.Add(ref current, 2);
                    mask4 |= Unsafe.Add(ref current, 3);

                    start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong) * 4);
                }
                while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

                end = ref Unsafe.AddByteOffset(ref end, sizeof(ulong) * 4);
            }

            if ((int)Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong) * 2)
            {
                mask1 |= Unsafe.ReadUnaligned<ulong>(ref start);
                mask1 |= Unsafe.ReadUnaligned<ulong>(ref Unsafe.AddByteOffset(ref start, sizeof(ulong)));

                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong) * 2);
            }

            if ((int)Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong))
            {
                mask1 |= Unsafe.ReadUnaligned<ulong>(ref start);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }

            if ((int)Unsafe.ByteOffset(ref start, ref end) >= sizeof(uint))
            {
                mask1 |= Unsafe.ReadUnaligned<uint>(ref start);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(uint));
            }

            if ((int)Unsafe.ByteOffset(ref start, ref end) >= sizeof(ushort))
            {
                mask1 |= Unsafe.ReadUnaligned<ushort>(ref start);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ushort));
            }

            if (Unsafe.IsAddressLessThan(ref start, ref end))
            {
                mask1 |= start;
            }

            return ((mask1 | mask2 | mask3 | mask4) & 0x8080808080808080) == 0;
        }
    }
}
