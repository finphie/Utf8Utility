using System;
using System.Diagnostics.CodeAnalysis;

namespace Utf8Utility
{
    /// <summary>
    /// UTF-8文字列用の読み取り専用Dictionary
    /// </summary>
    /// <typeparam name="TValue">Dictionary内部の値の型</typeparam>
    [SuppressMessage("Naming", "CA1711:識別子は、不適切なサフィックスを含むことはできません", Justification = "Dictionary")]
    public interface IReadOnlyUtf8StringDictionary<TValue>
    {
        /// <summary>
        /// 指定されたキーに対する値を取得します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        /// <returns>
        /// 指定されたキーが存在した場合は<see langword="true"/>、
        /// それ以外の場合は<see langword="false"/>。
        /// </returns>
        bool TryGetValue(Utf8String key, [MaybeNullWhen(false)] out TValue value);

        /// <summary>
        /// 指定されたキーに対する値を取得します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        /// <returns>
        /// 指定されたキーが存在した場合は<see langword="true"/>、
        /// それ以外の場合は<see langword="false"/>。
        /// </returns>
        bool TryGetValue(ReadOnlySpan<byte> key, [MaybeNullWhen(false)] out TValue value);
    }
}