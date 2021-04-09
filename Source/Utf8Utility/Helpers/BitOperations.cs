using System.Runtime.CompilerServices;

#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif

namespace Utf8Utility.Helpers
{
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
        public static int RoundUpPowerOfTwo(int value)
        {
#if NET5_0_OR_GREATER
            return Lzcnt.IsSupported || ArmBase.IsSupported || X86Base.IsSupported
                ? 1 << (32 - System.Numerics.BitOperations.LeadingZeroCount((uint)(value - 1)))
                : SoftwareFallback(value);
#else
            return SoftwareFallback(value);
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int SoftwareFallback(int value)
            {
                value--;
                value |= value >> 1;
                value |= value >> 2;
                value |= value >> 4;
                value |= value >> 8;
                value |= value >> 16;
                return ++value;
            }
        }
    }
}