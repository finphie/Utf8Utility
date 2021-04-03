﻿using System;
using System.Text;
using Microsoft.Toolkit.HighPerformance;

namespace Utf8Utility
{
    /// <summary>
    /// UTF-8を表す構造体です。
    /// </summary>
    public readonly struct Utf8String : IEquatable<Utf8String>
    {
        /// <summary>
        /// 空文字列を表す<see cref="Utf8String"/>インスタンスを取得します。
        /// </summary>
        /// <value>
        /// 空文字列を表す<see cref="Utf8String"/>インスタンス
        /// </value>
        public static readonly Utf8String Empty;

        readonly byte[] _value;

        /// <summary>
        /// <see cref="Utf8String"/>構造体の新しいインスタンスを取得します。
        /// </summary>
        /// <param name="bytes">UTF-8でエンコードされた<see cref="byte"/>配列</param>
        public Utf8String(byte[] bytes) => _value = bytes;

        /// <summary>
        /// <see cref="Utf8String"/>構造体の新しいインスタンスを取得します。
        /// </summary>
        /// <param name="s">文字列</param>
        public Utf8String(string s) => _value = Encoding.UTF8.GetBytes(s);

        /// <summary>
        /// UTF-8バイト数を取得します。
        /// </summary>
        /// <value>
        /// UTF-8バイト数
        /// </value>
        public int Length => _value.Length;

        /// <summary>
        /// i byte目を取得します。
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>i byte目の<see cref="byte"/></returns>
        public byte this[int index] => _value[index];

#if NET5_0_OR_GREATER
        /// <summary>
        /// i byte目を取得します。
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>i byte目の<see cref="byte"/></returns>
        public ReadOnlySpan<byte> this[Index index] => _value.AsSpan(index);

        /// <summary>
        /// 指定された範囲の<see cref="byte"/>配列を取得します。
        /// </summary>
        /// <param name="range">範囲</param>
        /// <returns>指定された範囲の<see cref="byte"/>配列</returns>
        public ReadOnlySpan<byte> this[Range range] => _value.AsSpan(range);
#endif

        /// <summary>
        /// 指定されたインスタンスのオブジェクトが等しいかどうかを表します。
        /// </summary>
        /// <param name="left"><see cref="Utf8String"/>インスタンス</param>
        /// <param name="right">比較対象の><see cref="Utf8String"/>インスタンス</param>
        /// <returns>
        /// 同じ文字列を表す場合は<see langword="true"/>、
        /// 異なる場合は、<see langword="false"/>。
        /// </returns>
        public static bool operator ==(Utf8String left, Utf8String right) => left.Equals(right);

        /// <summary>
        /// 指定されたインスタンスのオブジェクトが異なるかどうかを表します。
        /// </summary>
        /// <param name="left"><see cref="Utf8String"/>インスタンス</param>
        /// <param name="right">比較対象の><see cref="Utf8String"/>インスタンス</param>
        /// <returns>
        /// 異なる文字列を表す場合は<see langword="true"/>、
        /// 同じ場合は、<see langword="false"/>。
        /// </returns>
        public static bool operator !=(Utf8String left, Utf8String right) => !(left == right);

        /// <inheritdoc/>
        public override bool Equals(object? obj)
            => obj is Utf8String x && Equals(x);

        /// <inheritdoc/>
        public bool Equals(Utf8String other)
            => _value.AsSpan().SequenceEqual(other._value);

        /// <inheritdoc/>
        public override int GetHashCode() => _value.GetDjb2HashCode();

        /// <summary>
        /// <see cref="ReadOnlySpan{Byte}"/>構造体を取得します。
        /// </summary>
        /// <returns><see cref="ReadOnlySpan{Byte}"/>構造体</returns>
        public ReadOnlySpan<byte> AsSpan() => _value;

        /// <summary>
        /// i byte目を取得します。
        /// このメソッドは、境界チェックを行いません。
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>i byte目の<see cref="byte"/></returns>
        public byte DangerousGetByte(int index) => _value.DangerousGetReferenceAt(index);
    }
}