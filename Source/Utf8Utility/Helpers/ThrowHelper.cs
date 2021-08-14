using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Utf8Utility.Helpers;

/// <summary>
/// 例外をスローするためのヘルパークラスです。
/// </summary>
static class ThrowHelper
{
    /// <summary>
    /// 新しい<see cref="ArgumentOutOfRangeException"/>例外をスローします。
    /// </summary>
    /// <param name="paramName">引数名</param>
    /// <exception cref="ArgumentOutOfRangeException">常にこの例外をスローします。</exception>
    [DebuggerHidden]
    [DoesNotReturn]
    public static void ThrowArgumentOutOfRangeException(string paramName)
        => throw new ArgumentOutOfRangeException(paramName);
}