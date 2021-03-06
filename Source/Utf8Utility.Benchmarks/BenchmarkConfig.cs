using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;

namespace Utf8Utility.Benchmarks;

public sealed class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddExporter(MarkdownExporter.GitHub);
        AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory);
        AddColumn(CategoriesColumn.Default);
        AddDiagnoser(MemoryDiagnoser.Default);

        AddJob(Job.Default.WithRuntime(CoreRuntime.CreateForNewVersion("net7.0", ".NET 7.0")));
        AddJob(Job.Default.WithRuntime(CoreRuntime.Core60));
    }
}
