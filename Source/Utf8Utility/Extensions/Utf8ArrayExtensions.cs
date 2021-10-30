﻿using System.Runtime.CompilerServices;
using Utf8Utility.Helpers;

namespace Utf8Utility.Extensions;

/// <summary>
/// <see cref="Utf8Array"/>構造体の拡張メソッドです。
/// </summary>
public static class Utf8ArrayExtensions
{
    /// <summary>
    /// 最初の1文字のバイト数を取得します。
    /// </summary>
    /// <param name="array">UTF-8配列</param>
    /// <returns>最初の1文字のバイト数</returns>
    /// <exception cref="ArgumentException">UTF-8配列が空の場合、この例外をスローします。</exception>
    /// <exception cref="InvalidOperationException">バイト数の取得に失敗した場合、この例外をスローします。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetFirstCharByteCount(this byte value)
    {
        //ThrowHelper.ThrowArgumentExceptionIfEmpty(array);

        //var first = array.DangerousGetReference();

        if (value <= 0x7F)
        {
            return 1;
        }

        if ((value & 0xE0) == 0xC0)
        {
            return 2;
        }

        if ((value & 0xF0) == 0xE0)
        {
            return 3;
        }

        if ((value & 0xF8) == 0xF0)
        {
            return 4;
        }

        ThrowHelper.ThrowInvalidUtf8SequenceException(nameof(value));
        return default;
    }
}
