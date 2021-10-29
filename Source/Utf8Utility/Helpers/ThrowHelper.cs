using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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

    /// <summary>
    /// 新しい<see cref="InvalidOperationException"/>例外をスローします。
    /// </summary>
    /// <param name="paramName">引数名</param>
    /// <exception cref="InvalidOperationException">常にこの例外をスローします。</exception>
    [DebuggerHidden]
    [DoesNotReturn]
    public static void ThrowInvalidUtf8SequenceException(string paramName)
        => throw new InvalidOperationException($"Invalid UTF-8 sequence in argument '{paramName}'.");

    /// <summary>
    /// 対象のUTF-8配列が空の場合、新しい<see cref="ArgumentException"/>例外をスローします。
    /// </summary>
    /// <param name="argument">対象の文字列</param>
    /// <param name="paramName">引数名</param>
    /// <exception cref="ArgumentException">常にこの例外をスローします。</exception>
    [DebuggerHidden]
    public static void ThrowArgumentExceptionIfEmpty(Utf8Array argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument == Utf8Array.Empty)
        {
            throw new ArgumentException($"The argument '{paramName}' cannot be empty.", paramName);
        }
    }
}
