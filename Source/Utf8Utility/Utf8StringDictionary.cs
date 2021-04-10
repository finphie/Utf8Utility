using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.HighPerformance;
using Utf8Utility.Helpers;

namespace Utf8Utility
{
    /// <summary>
    /// <see cref="Utf8String"/>をキーにしたDictionaryです。
    /// </summary>
    /// <remarks>
    /// Microsoft.Collections.Extensions.DictionarySlimを参考に実装しました。
    /// </remarks>
    /// <typeparam name="TValue">Dictionary内部の値の型</typeparam>
    [DebuggerDisplay($"Count = {nameof(Count)}")]
    [SuppressMessage("Naming", "CA1711:識別子は、不適切なサフィックスを含むことはできません", Justification = "Dictionary")]
    public sealed class Utf8StringDictionary<TValue>
    {
        static readonly Entry[] InitialEntries = new Entry[1];
        static readonly int[] SizeOneIntArray = new int[1];

        int _freeList = -1;
        int[] _buckets;
        Entry[] _entries;

        /// <summary>
        /// <see cref="Utf8StringDictionary{TValue}"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        public Utf8StringDictionary()
        {
            _buckets = SizeOneIntArray;
            _entries = InitialEntries;
        }

        /// <summary>
        /// <see cref="Utf8StringDictionary{TValue}"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="capacity">格納できる要素数の初期値</param>
        public Utf8StringDictionary(int capacity)
        {
            if (capacity < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(capacity));
            }

            if (capacity < 2)
            {
                capacity = 2;
            }

            capacity = BitOperations.RoundUpPowerOfTwo(capacity);
            _buckets = new int[capacity];
            _entries = new Entry[capacity];
        }

        /// <summary>
        /// 要素数を取得します。
        /// </summary>
        /// <value>
        /// 要素数
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// 要素を追加します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        public void Add(Utf8String key, TValue value)
        {
            var entries = _entries;
            var bucketIndex = GetBucketIndex(key.GetHashCode());

            for (var i = GetBucket(bucketIndex) - 1; (uint)i < (uint)entries.Length; i = entries[i].Next)
            {
                ref var entry = ref entries[i];

                if (key == entry.Key)
                {
                    entry.Value = value;
                    return;
                }
            }

            AddKey(key, bucketIndex) = value;
        }

        /// <summary>
        /// 指定されたキーに対する値を取得します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        /// <returns>
        /// 指定されたキーが存在した場合は<see langword="true"/>、
        /// それ以外の場合は<see langword="false"/>。
        /// </returns>
        public bool TryGetValue(Utf8String key, [MaybeNullWhen(false)] out TValue value)
        {
            var entries = _entries;
            var bucketIndex = GetBucketIndex(key.GetHashCode());

            for (var i = GetBucket(bucketIndex) - 1; (uint)i < (uint)entries.Length; i = entries[i].Next)
            {
                ref var entry = ref entries[i];

                if (key == entry.Key)
                {
                    value = entry.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 指定されたキーに対する値を取得します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        /// <returns>
        /// 指定されたキーが存在した場合は<see langword="true"/>、
        /// それ以外の場合は<see langword="false"/>。
        /// </returns>
        public bool TryGetValue(ReadOnlySpan<byte> key, [MaybeNullWhen(false)] out TValue value)
        {
            var entries = _entries;
            var bucketIndex = GetBucketIndex(key.GetDjb2HashCode());

            for (var i = GetBucket(bucketIndex) - 1; (uint)i < (uint)entries.Length; i = entries[i].Next)
            {
                ref var entry = ref entries[i];

                if (key.SequenceEqual(entry.Key.AsSpan()))
                {
                    value = entry.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// すべてのキーと値を削除します。
        /// </summary>
        public void Clear()
        {
            Count = 0;
            _freeList = -1;
            _buckets = SizeOneIntArray;
            _entries = InitialEntries;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetBucketIndex(int hashCode)
            => hashCode & (_buckets.Length - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ref int GetBucket(int bucketIndex)
            => ref _buckets.DangerousGetReferenceAt(bucketIndex);

        ref TValue AddKey(Utf8String key, int bucketIndex)
        {
            var entries = _entries;
            int entryIndex;

            if (_freeList != -1)
            {
                entryIndex = _freeList;
                _freeList = -3 - entries.DangerousGetReferenceAt(_freeList).Next;
            }
            else
            {
                if (Count == entries.Length || entries.Length == 1)
                {
                    entries = Resize();
                    bucketIndex = GetBucketIndex(key.GetHashCode());
                }

                entryIndex = Count;
            }

            ref var entry = ref entries.DangerousGetReferenceAt(entryIndex);
            ref var bucket = ref _buckets.DangerousGetReferenceAt(bucketIndex);
            entry.Key = key;
            entry.Next = bucket - 1;
            bucket = entryIndex + 1;
            Count++;

            return ref entry.Value;
        }

        Entry[] Resize()
        {
            var count = Count;
            var newSize = checked(_entries.Length * 2);

            var entries = new Entry[newSize];
            Array.Copy(_entries, 0, entries, 0, count);

            var newBuckets = new int[entries.Length];
            while (count-- > 0)
            {
                ref var entry = ref entries.DangerousGetReferenceAt(count);
                var bucketIndex = entry.Key.GetHashCode() & (newBuckets.Length - 1);
                ref var newBucket = ref newBuckets.DangerousGetReferenceAt(bucketIndex);
                entry.Next = newBucket - 1;
                newBucket = count + 1;
            }

            _buckets = newBuckets;
            _entries = entries;

            return entries;
        }

        [DebuggerDisplay("({key}, {value})->{next}")]
        struct Entry
        {
            public Utf8String Key;
            public TValue Value;
            public int Next;
        }
    }
}