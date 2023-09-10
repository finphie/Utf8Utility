using System.Runtime.CompilerServices;

namespace Utf8Utility.Helpers;

/// <summary>
/// bit操作に関するヘルパークラスです。
/// </summary>
static class BitOperations
{
    /// <summary>
    /// 1になっているビットの数を取得します。
    /// </summary>
    /// <param name="value">値</param>
    /// <returns>1になっているビットの数を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PopCount(ulong value)
#if NET7_0_OR_GREATER
        => System.Numerics.BitOperations.PopCount(value);
#else
    {
        return SoftwareFallback(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int SoftwareFallback(ulong value)
        {
            const ulong C1 = 0x_55555555_55555555;
            const ulong C2 = 0x_33333333_33333333;
            const ulong C3 = 0x_0F0F0F0F_0F0F0F0F;
            const ulong C4 = 0x_01010101_01010101;

            value -= (value >> 1) & C1;
            value = (value & C2) + ((value >> 2) & C2);
            value = (((value + (value >> 4)) & C3) * C4) >> 56;

            return (int)value;
        }
    }
#endif

    /// <summary>
    /// 指定された数値を2の累乗になるように繰り上げます。
    /// </summary>
    /// <param name="value">数値</param>
    /// <returns>2の累乗を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundUpToPowerOf2(int value)
#if NET7_0_OR_GREATER
        => (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)value);
#else
    {
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return ++value;
    }
#endif
}
