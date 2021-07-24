using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
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
    [DebuggerDisplay($"Count = {{{nameof(Count)}}}")]
    [SuppressMessage("Naming", "CA1711:識別子は、不適切なサフィックスを含むことはできません", Justification = "Dictionary")]
    public sealed class Utf8StringDictionary<TValue> : IUtf8StringDictionary<TValue>, IReadOnlyUtf8StringDictionary<TValue>
    {
        int _freeList = -1;
        int[] _buckets;
        Entry[] _entries;

        /// <summary>
        /// <see cref="Utf8StringDictionary{TValue}"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        public Utf8StringDictionary()
        {
            _buckets = new int[2];
            _entries = new Entry[2];
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

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(Utf8String key, TValue value)
        {
            var entries = _entries;
            var bucketIndex = GetBucketIndex(key.GetHashCode());
            var i = GetBucket(bucketIndex) - 1;

            do
            {
                // 境界チェック削除のためにdo-while文の必要がある。
                // https://github.com/dotnet/runtime/issues/9422
                if ((uint)i >= (uint)entries.Length)
                {
                    break;
                }

                ref var entry = ref entries[i];

                if (key == entry.Key)
                {
                    return false;
                }

                i = entry.Next;
            }
            while (true);

            AddKey(key, bucketIndex) = value;
            return true;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(Utf8String key, [MaybeNullWhen(false)] out TValue value)
            => TryGetValue(key.AsSpan(), out value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(ReadOnlySpan<byte> key, [MaybeNullWhen(false)] out TValue value)
        {
            ref var valueRef = ref GetValueRefOrNullRef(key);

            if (Unsafe.IsNullRef(ref valueRef))
            {
                value = default;
                return false;
            }

            value = valueRef;
            return true;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out TValue value)
        {
            int count;

#if NET5_0_OR_GREATER
            count = Encoding.UTF8.GetByteCount(key);
#else
            if (key.IsEmpty)
            {
                return TryGetValue(ReadOnlySpan<byte>.Empty, out value);
            }

            unsafe
            {
                fixed (char* chars = key)
                {
                    count = Encoding.UTF8.GetByteCount(chars, key.Length);
                }
            }
#endif

            byte[]? rentedUtf8Key = null;
            Span<byte> utf8Key = count <= 256
                ? stackalloc byte[count]
                : (rentedUtf8Key = ArrayPool<byte>.Shared.Rent(count));

            try
            {
#if NET5_0_OR_GREATER
                Encoding.UTF8.GetBytes(key, utf8Key);
#else
                unsafe
                {
                    fixed (char* chars = key)
                    {
                        fixed (byte* bytes = utf8Key)
                        {
                            Encoding.UTF8.GetBytes(chars, key.Length, bytes, utf8Key.Length);
                        }
                    }
                }
#endif

                return TryGetValue(utf8Key, out value);
            }
            finally
            {
                if (rentedUtf8Key is not null)
                {
                    ArrayPool<byte>.Shared.Return(rentedUtf8Key);
                }
            }
        }

        /// <summary>
        /// キーが存在する場合は<typeparamref name="TValue"/>への参照、存在しない場合はnull参照を返します。
        /// </summary>
        /// <param name="key">ルックアップに使用されるキー。</param>
        /// <returns>
        /// キーが存在する場合は<typeparamref name="TValue"/>への参照、
        /// それ以外の場合はNull参照。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrNullRef(Utf8String key)
            => ref GetValueRefOrNullRef(key.AsSpan());

        /// <summary>
        /// キーが存在する場合は<typeparamref name="TValue"/>への参照、存在しない場合はnull参照を返します。
        /// </summary>
        /// <param name="key">ルックアップに使用されるキー。</param>
        /// <returns>
        /// キーが存在する場合は<typeparamref name="TValue"/>への参照、
        /// それ以外の場合はNull参照。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrNullRef(ReadOnlySpan<byte> key)
        {
            var entries = _entries;
            var bucketIndex = GetBucketIndex(key.GetDjb2HashCode());
            var i = GetBucket(bucketIndex) - 1;

            do
            {
                // 境界チェック削除のためにdo-while文の必要がある。
                // https://github.com/dotnet/runtime/issues/9422
                if ((uint)i >= (uint)entries.Length)
                {
                    break;
                }

                ref var entry = ref entries[i];

                if (key.SequenceEqual(entry.Key.AsSpan()))
                {
                    return ref entry.Value;
                }

                i = entry.Next;
            }
            while (true);

            return ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        /// すべてのキーと値を削除します。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _buckets.AsSpan().Clear();
            _entries.AsSpan(0, Count).Clear();

            Count = 0;
            _freeList = -1;
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
                if (Count == entries.Length)
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

        [DebuggerDisplay("({Key}, {Value})->{Next}")]
        struct Entry
        {
            public Utf8String Key;
            public TValue Value;
            public int Next;
        }
    }
}