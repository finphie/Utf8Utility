// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Utf8Utility.Helpers;

/// <summary>
/// Unicode関連のヘルパークラスです。
/// </summary>
public static class UnicodeUtility
{
    /// <summary>
    /// Given a Unicode scalar value, gets the number of UTF-8 code units required to represent this value.
    /// </summary>
    /// <param name="value">値</param>
    /// <returns>指定されたUTF-8値のバイト数</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetUtf8SequenceLength(uint value)
    {
        // The logic below can handle all valid scalar values branchlessly.
        // It gives generally good performance across all inputs, and on x86
        // it's only six instructions: lea, sar, xor, add, shr, lea.

        // 'a' will be -1 if input is < 0x800; else 'a' will be 0
        // => 'a' will be -1 if input is 1 or 2 UTF-8 code units; else 'a' will be 0
        var a = ((int)value - 0x0800) >> 31;

        // The number of UTF-8 code units for a given scalar is as follows:
        // - U+0000..U+007F => 1 code unit
        // - U+0080..U+07FF => 2 code units
        // - U+0800..U+FFFF => 3 code units
        // - U+10000+       => 4 code units
        //
        // If we XOR the incoming scalar with 0xF800, the chart mutates:
        // - U+0000..U+F7FF => 3 code units
        // - U+F800..U+F87F => 1 code unit
        // - U+F880..U+FFFF => 2 code units
        // - U+10000+       => 4 code units
        //
        // Since the 1- and 3-code unit cases are now clustered, they can
        // both be checked together very cheaply.
        value ^= 0xF800u;
        value -= 0xF880u;   // if scalar is 1 or 3 code units, high byte = 0xFF; else high byte = 0x00
        value += 4 << 24;   // if scalar is 1 or 3 code units, high byte = 0x03; else high byte = 0x04
        value >>= 24;       // shift high byte down

        // Final return value:
        // - U+0000..U+007F => 3 + (-1) * 2 = 1
        // - U+0080..U+07FF => 4 + (-1) * 2 = 2
        // - U+0800..U+FFFF => 3 + ( 0) * 2 = 3
        // - U+10000+       => 4 + ( 0) * 2 = 4
        return (int)value + (a * 2);
    }
}
