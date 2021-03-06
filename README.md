# Utf8Utility

[![Build(.NET)](https://github.com/finphie/Utf8Utility/actions/workflows/build-dotnet.yml/badge.svg)](https://github.com/finphie/Utf8Utility/actions/workflows/build-dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/Utf8Utility?color=0078d4&label=NuGet)](https://www.nuget.org/packages/Utf8Utility/)
[![Azure Artifacts](https://feeds.dev.azure.com/finphie/7af9aa4d-c550-43af-87a5-01539b2d9934/_apis/public/Packaging/Feeds/18cbb017-6f1d-41eb-b9a5-a6dbf411e3f7/Packages/72c69351-0c11-40f2-8853-5712bf32468d/Badge)](https://dev.azure.com/finphie/Main/_packaging?_a=package&feed=18cbb017-6f1d-41eb-b9a5-a6dbf411e3f7&package=72c69351-0c11-40f2-8853-5712bf32468d&preferRelease=true)

UTF-8関連のユーティリティライブラリです。

## 説明

Utf8Utilityは、UTF-8関連処理の実装を詰め合わせたライブラリです。

## インストール

### NuGet（正式リリース版）

```console
dotnet add package Utf8Utility
```

### Azure Artifacts（開発用ビルド）

```console
dotnet add package Utf8Utility -s https://pkgs.dev.azure.com/finphie/Main/_packaging/DotNet/nuget/v3/index.json
```

## 使い方

```csharp
using System;
using Utf8Utility;
using Utf8Utility.Text;

// stringまたはUTF-8のバイト配列、ReadOnlySpan{char|byte}を指定できます。
var array = new Utf8Array("abc");

// バイト数
var byteCount = array.ByteCount;

// 文字数
var length = array.GetLength();

// 空かどうか
var isEmpty = array.IsEmpty;

// 空か空白文字列かどうか
var isEmptyOrWhiteSpace = array.IsEmptyOrWhiteSpace();

// Ascii文字列かどうか
var isAscii = array.IsAscii();

// 内部配列への参照
ref var start = ref array.DangerousGetReference();

// 比較
var compareTo = array.CompareTo(array);
Utf8Array.CompareOrdinal(array, array);
Utf8Array.Compare(array, array, StringComparison.CurrentCulture);

var empty = Utf8Array.Empty;
var equals = array.Equals(array);
var hash = array.GetHashCode();
var utf16 = array.ToString();
var span = array.AsSpan();
array.TryFormat(stackalloc char[256], out var charsWritten);

// Utf8ArrayをキーとしたDictionaryです。
var dict = new Utf8ArrayDictionary<int>();

// キー指定にはUtf8Arrayの他にReadOnlySpan{char|byte}を指定できます。
dict.TryGetValue(array, out var result);
ref var dictStart = ref dict.GetValueRefOrNullRef(array);

dict.TryAdd(array, 1);
dict.Clear();

// Ascii文字列かどうか
var span = array.AsSpan();
var isAscii = UnicodeUtility.IsAscii(span);
```

## サポートフレームワーク

- .NET 7
- .NET 6
- .NET Standard 2.1

## 作者

finphie

## ライセンス

MIT

## クレジット

このプロジェクトでは、次のライブラリ等を使用しています。

### ライブラリ

- [CommunityToolkit.HighPerformance](https://github.com/CommunityToolkit/dotnet)

### テスト

- [FluentAssertions](https://github.com/fluentassertions/fluentassertions)
- [Microsoft.NET.Test.Sdk](https://github.com/microsoft/vstest)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [NuGet.Frameworks](https://github.com/NuGet/NuGet.Client)
- [xunit](https://github.com/xunit/xunit)
- [xunit.runner.visualstudio](https://github.com/xunit/visualstudio.xunit)

### アナライザー

- Microsoft.CodeAnalysis.NetAnalyzers (SDK組み込み)
- [Microsoft.VisualStudio.Threading.Analyzers](https://github.com/Microsoft/vs-threading)
- [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)

### ベンチマーク

- [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)
- [CommandLineParser](https://github.com/commandlineparser/commandline)
- [Iced](https://github.com/icedland/iced)
- [Microsoft.CodeAnalysis.CSharp](https://github.com/dotnet/roslyn)
- [Microsoft.Diagnostics.NETCore.Client](https://github.com/dotnet/diagnostics)
- [Microsoft.Diagnostics.Runtime](https://github.com/Microsoft/clrmd)
- [Microsoft.Diagnostics.Tracing.TraceEvent](https://github.com/Microsoft/perfview)
- [Perfolizer](https://github.com/AndreyAkinshin/perfolizer)

### その他

- [Microsoft.SourceLink.GitHub](https://github.com/dotnet/sourcelink)
