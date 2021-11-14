using System.Runtime.CompilerServices;

namespace Utf8Utility.Helpers;

/// <summary>
/// bit操作に関するヘルパークラスです。
/// </summary>
static class BitOperations
{
    /// <summary>
    /// 指定された数値を2の累乗になるように繰り上げます。
    /// </summary>
    /// <param name="value">数値</param>
    /// <returns>2の累乗</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundUpToPowerOf2(int value)
#if NET6_0_OR_GREATER
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

#if NET6_0_OR_GREATER
    /// <summary>
    /// 立っているビット数を取得します。
    /// </summary>
    /// <param name="value">数値</param>
    /// <returns>立っているビット数</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PopCount(ulong value) => System.Numerics.BitOperations.PopCount(value);
#endif
}
