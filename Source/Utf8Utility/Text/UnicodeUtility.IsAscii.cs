using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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

#if NET6_0_OR_GREATER
        if (Avx2.IsSupported)
        {
            return IsAsciiAvx2(value);
        }

        if (Sse2.IsSupported)
        {
            return IsAsciiSse2(value);
        }
#endif

        return IsAsciiUlong(value);
    }

#if NET6_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsAsciiAvx2(ReadOnlySpan<byte> value)
    {
        nuint index = 0;
        var mask32 = Vector256<byte>.Zero;

        ref var first = ref MemoryMarshal.GetReference(value);

        if (value.Length >= Vector256<byte>.Count)
        {
            var endIndex = value.Length - Vector256<byte>.Count;
            ref var current = ref Unsafe.As<byte, Vector256<byte>>(ref first);

            do
            {
                mask32 = Avx2.Or(mask32, Unsafe.AddByteOffset(ref current, index));
                index += (uint)Vector256<byte>.Count;
            }
            while ((int)index <= endIndex);
        }

        var result = Avx2.MoveMask(mask32);
        var mask8 = 0UL;

        if (value.Length - (int)index >= sizeof(ulong) * 2)
        {
            ref var current = ref Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, index));

            mask8 = current;
            mask8 |= Unsafe.Add(ref current, 1);
            index += sizeof(ulong) * 2;
        }

        if (value.Length - (int)index >= sizeof(ulong))
        {
            mask8 |= Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ulong);
        }

        if (value.Length - (int)index >= sizeof(uint))
        {
            mask8 |= Unsafe.As<byte, uint>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(uint);
        }

        if (value.Length - (int)index >= sizeof(ushort))
        {
            mask8 |= Unsafe.As<byte, ushort>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ushort);
        }

        if ((int)index < value.Length)
        {
            mask8 |= Unsafe.AddByteOffset(ref first, index);
        }

        return ((uint)result | (mask8 & 0x8080808080808080)) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsAsciiSse2(ReadOnlySpan<byte> value)
    {
        nuint index = 0;
        var mask16 = Vector128<byte>.Zero;

        ref var first = ref MemoryMarshal.GetReference(value);

        if (value.Length >= Vector128<byte>.Count)
        {
            var endIndex = value.Length - Vector128<byte>.Count;
            ref var current = ref Unsafe.As<byte, Vector128<byte>>(ref first);

            do
            {
                mask16 = Sse2.Or(mask16, Unsafe.AddByteOffset(ref current, index));
                index += (uint)Vector128<byte>.Count;
            }
            while ((int)index <= endIndex);
        }

        var result = Sse2.MoveMask(mask16);
        var mask8 = 0UL;

        if (value.Length - (int)index >= sizeof(ulong))
        {
            mask8 |= Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ulong);
        }

        if (value.Length - (int)index >= sizeof(uint))
        {
            mask8 |= Unsafe.As<byte, uint>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(uint);
        }

        if (value.Length - (int)index >= sizeof(ushort))
        {
            mask8 |= Unsafe.As<byte, ushort>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ushort);
        }

        if ((int)index < value.Length)
        {
            mask8 |= Unsafe.AddByteOffset(ref first, index);
        }

        return ((uint)result | (mask8 & 0x8080808080808080)) == 0;
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsAsciiUlong(ReadOnlySpan<byte> value)
    {
        nuint index = 0;
        var maskA = 0UL;
        var maskB = 0UL;
        var maskC = 0UL;
        var maskD = 0UL;

        ref var first = ref MemoryMarshal.GetReference(value);

        if (value.Length >= sizeof(ulong) * 4)
        {
            ref var current = ref Unsafe.As<byte, ulong>(ref first);
            var endIndex = value.Length - (sizeof(ulong) * 4);

            do
            {
#if NET6_0_OR_GREATER
                maskA |= Unsafe.AddByteOffset(ref current, index);
                maskB |= Unsafe.AddByteOffset(ref current, index + sizeof(ulong));
                maskC |= Unsafe.AddByteOffset(ref current, index + (sizeof(ulong) * 2));
                maskD |= Unsafe.AddByteOffset(ref current, index + (sizeof(ulong) * 3));
#else
                maskA |= Unsafe.AddByteOffset(ref current, (nint)index);
                maskB |= Unsafe.AddByteOffset(ref current, (nint)(index + sizeof(ulong)));
                maskC |= Unsafe.AddByteOffset(ref current, (nint)(index + (sizeof(ulong) * 2)));
                maskD |= Unsafe.AddByteOffset(ref current, (nint)(index + (sizeof(ulong) * 3)));
#endif
                index += sizeof(ulong) * 4;
            }
            while ((int)index < endIndex);
        }

        if (value.Length - (int)index >= sizeof(ulong) * 2)
        {
#if NET6_0_OR_GREATER
            ref var current = ref Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, index));
#else
            ref var current = ref Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, (nint)index));
#endif
            maskA |= current;
            maskA |= Unsafe.Add(ref current, 1);
            index += sizeof(ulong) * 2;
        }

        if (value.Length - (int)index >= sizeof(ulong))
        {
#if NET6_0_OR_GREATER
            maskA |= Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, index));
#else
            maskA |= Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, (nint)index));
#endif
            index += sizeof(ulong);
        }

        if (value.Length - (int)index >= sizeof(uint))
        {
#if NET6_0_OR_GREATER
            maskA |= Unsafe.As<byte, uint>(ref Unsafe.AddByteOffset(ref first, index));
#else
            maskA |= Unsafe.As<byte, uint>(ref Unsafe.AddByteOffset(ref first, (nint)index));
#endif
            index += sizeof(uint);
        }

        if (value.Length - (int)index >= sizeof(ushort))
        {
#if NET6_0_OR_GREATER
            maskA |= Unsafe.As<byte, ushort>(ref Unsafe.AddByteOffset(ref first, index));
#else
            maskA |= Unsafe.As<byte, ushort>(ref Unsafe.AddByteOffset(ref first, (nint)index));
#endif
            index += sizeof(ushort);
        }

        if ((int)index < value.Length)
        {
#if NET6_0_OR_GREATER
            maskA |= Unsafe.AddByteOffset(ref first, index);
#else
            maskA |= Unsafe.AddByteOffset(ref first, (nint)index);
#endif
        }

        return ((maskA | maskB | maskC | maskD) & 0x8080808080808080) == 0;
    }
}
