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

        public Utf8StringDictionary()
        {
            _buckets = SizeOneIntArray;
            _entries = InitialEntries;
        }

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

        public int Count { get; private set; }

        public void Clear()
        {
            Count = 0;
            _freeList = -1;
            _buckets = SizeOneIntArray;
            _entries = InitialEntries;
        }

        public void Add(Utf8String key, TValue value)
        {
            var bucketIndex = GetBucketIndex(key.GetHashCode());
            ref var valueRef = ref FindValue(key, bucketIndex);

            if (Unsafe.IsNullRef(ref valueRef))
            {
                AddKey(key, bucketIndex) = value;
            }
            else
            {
                valueRef = value;
            }
        }

        public bool TryGetValue(Utf8String key, [MaybeNullWhen(false)] out TValue value)
        {
            var bucketIndex = GetBucketIndex(key.GetHashCode());
            ref var valueRef = ref FindValue(key, bucketIndex);

            if (Unsafe.IsNullRef(ref valueRef))
            {
                value = default;
                return false;
            }

            value = valueRef;
            return true;
        }

        public bool TryGetValue(ReadOnlySpan<byte> key, [MaybeNullWhen(false)] out TValue value)
        {
            var bucketIndex = GetBucketIndex(key.GetDjb2HashCode());
            ref var valueRef = ref FindValue(key, bucketIndex);

            if (Unsafe.IsNullRef(ref valueRef))
            {
                value = default;
                return false;
            }

            value = valueRef;
            return true;
        }

        internal ref TValue FindValue(Utf8String key, int bucketIndex)
        {
            var entries = _entries;

            for (var i = GetBucket(bucketIndex) - 1; (uint)i < (uint)entries.Length; i = entries[i].Next)
            {
                ref var entry = ref entries[i];

                if (key == entry.Key)
                {
                    return ref entry.Value;
                }
            }

            return ref Unsafe.NullRef<TValue>();
        }

        internal ref TValue FindValue(ReadOnlySpan<byte> key, int bucketIndex)
        {
            var entries = _entries;

            for (var i = GetBucket(bucketIndex) - 1; (uint)i < (uint)entries.Length; i = entries[i].Next)
            {
                ref var entry = ref entries[i];

                if (key.SequenceEqual(entry.Key.AsSpan()))
                {
                    return ref entry.Value;
                }
            }

            return ref Unsafe.NullRef<TValue>();
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
                _freeList = -3 - entries[_freeList].Next;
            }
            else
            {
                if (Count == entries.Length || entries.Length == 1)
                {
                    entries = Resize();
                    bucketIndex = key.GetHashCode() & (_buckets.Length - 1);
                }

                entryIndex = Count;
            }

            entries[entryIndex].Key = key;
            entries[entryIndex].Next = _buckets[bucketIndex] - 1;
            _buckets[bucketIndex] = entryIndex + 1;
            Count++;

            return ref entries[entryIndex].Value;
        }

        Entry[] Resize()
        {
            Debug.Assert(_entries.Length == Count || _entries.Length == 1);
            var count = Count;
            var newSize = _entries.Length * 2;
            if ((uint)newSize > int.MaxValue)
                throw new InvalidOperationException();

            var entries = new Entry[newSize];
            Array.Copy(_entries, 0, entries, 0, count);

            var newBuckets = new int[entries.Length];
            while (count-- > 0)
            {
                var bucketIndex = entries[count].Key.GetHashCode() & (newBuckets.Length - 1);
                entries[count].Next = newBuckets[bucketIndex] - 1;
                newBuckets[bucketIndex] = count + 1;
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