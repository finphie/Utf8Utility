using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Helpers;
using Utf8Utility.Helpers;

#if !NET8_0_OR_GREATER
using CommunityToolkit.Diagnostics;
#endif

namespace Utf8Utility;

/// <summary>
/// <see cref="Utf8Array"/>をキーにしたDictionaryです。
/// </summary>
/// <remarks>
/// <para>Microsoft.Collections.Extensions.DictionarySlimを参考に実装しました。</para>
/// </remarks>
/// <typeparam name="TValue">Dictionary内部の値の型</typeparam>
[DebuggerDisplay($"Count = {{{nameof(Count)}}}")]
[SuppressMessage("Naming", "CA1711:識別子は、不適切なサフィックスを含むことはできません", Justification = "Dictionary")]
public sealed class Utf8ArrayDictionary<TValue> : IUtf8ArrayDictionary<TValue>, IReadOnlyUtf8ArrayDictionary<TValue>
{
    int _freeList = -1;
    int[] _buckets;
    Entry[] _entries;

    /// <summary>
    /// <see cref="Utf8ArrayDictionary{TValue}"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    public Utf8ArrayDictionary()
    {
        _buckets = new int[2];
        _entries = new Entry[2];
    }

    /// <summary>
    /// <see cref="Utf8ArrayDictionary{TValue}"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="capacity">格納できる要素数の初期値</param>
    public Utf8ArrayDictionary(int capacity)
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
#else
        Guard.IsGreaterThanOrEqualTo(capacity, 0);
#endif

        if (capacity < 2)
        {
            capacity = 2;
        }

        capacity = BitOperations.RoundUpToPowerOf2(capacity);
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
    public bool TryAdd(Utf8Array key, TValue value)
    {
        var entries = _entries;
        var bucketIndex = GetBucketIndex(key.GetHashCode());
        var i = GetBucket(bucketIndex) - 1;

        do
        {
            // 境界チェック削除のためにdo-while文にする必要がある。
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
    public bool TryGetValue(Utf8Array key, [MaybeNullWhen(false)] out TValue value)
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
        var count = Encoding.UTF8.GetByteCount(key);

        byte[]? rentedUtf8Key = null;
        Span<byte> utf8Key = count <= 256
            ? stackalloc byte[count]
            : (rentedUtf8Key = ArrayPool<byte>.Shared.Rent(count));

        try
        {
            Encoding.UTF8.GetBytes(key, utf8Key);
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
    /// <param name="key">ルックアップに使用されるキー</param>
    /// <returns>
    /// キーが存在する場合は<typeparamref name="TValue"/>への参照、
    /// それ以外の場合はNull参照を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TValue GetValueRefOrNullRef(Utf8Array key)
        => ref GetValueRefOrNullRef(key.AsSpan());

    /// <summary>
    /// キーが存在する場合は<typeparamref name="TValue"/>への参照、存在しない場合はnull参照を返します。
    /// </summary>
    /// <param name="key">ルックアップに使用されるキー</param>
    /// <returns>
    /// キーが存在する場合は<typeparamref name="TValue"/>への参照、
    /// それ以外の場合はNull参照を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TValue GetValueRefOrNullRef(ReadOnlySpan<byte> key)
    {
        var entries = _entries;
        var bucketIndex = GetBucketIndex(HashCode<byte>.Combine(key));
        var i = GetBucket(bucketIndex) - 1;

        do
        {
            // 境界チェック削除のためにdo-while文にする必要がある。
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

    ref TValue AddKey(Utf8Array key, int bucketIndex)
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
        public Utf8Array Key;
        public TValue Value;
        public int Next;
    }
}
