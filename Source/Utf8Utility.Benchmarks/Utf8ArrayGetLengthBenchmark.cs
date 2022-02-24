using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Utf8Utility.Text;

namespace Utf8Utility.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class Utf8ArrayGetLengthBenchmark
{
    Utf8Array _value;

    [Params("abcd", "あいうえお", "あaαβaあααいうazzαああαabc")]
    public string? Value { get; set; }

    [Params(1, 1000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup() => _value = new(string.Concat(Enumerable.Repeat(Value, Count)));

    [Benchmark]
    public int GetLength_Loop()
    {
        var count = 0;
        nuint index = 0;

        while ((int)index < _value.ByteCount)
        {
            var value = Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index);

            if ((value & 0xC0) != 0x80)
            {
                count++;
            }

            index++;
        }

        return count;
    }

    [Benchmark]
    public int GetLength_Table()
    {
        // 最適化の関係でcount,iの順番で宣言する必要あり。
        var count = 0;
        nuint index = 0;

        while ((int)index < _value.ByteCount)
        {
            ref var valueStart = ref _value.DangerousGetReference();

            // 最適化の関係でrefローカル変数にしてはいけない。
            var value = Unsafe.AddByteOffset(ref valueStart, index);

            index += (uint)UnicodeUtility.GetUtf8SequenceLength(value);
            count++;
        }

        return count;
    }

    [Benchmark(Baseline = true)]
    public int GetLength() => _value.GetLength();
}
