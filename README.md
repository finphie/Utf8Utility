# Utf8Utility

[![Build(.NET)](https://github.com/finphie/Utf8Utility/actions/workflows/build-dotnet.yml/badge.svg)](https://github.com/finphie/Utf8Utility/actions/workflows/build-dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/Utf8Utility?color=0078d4&label=NuGet)](https://www.nuget.org/packages/Utf8Utility/)
[![Azure Artifacts](https://feeds.dev.azure.com/finphie/7af9aa4d-c550-43af-87a5-01539b2d9934/_apis/public/Packaging/Feeds/DotNet/Packages/72c69351-0c11-40f2-8853-5712bf32468d/Badge)](https://dev.azure.com/finphie/Main/_artifacts/feed/DotNet/NuGet/Utf8Utility?preferRelease=true)

UTF-8関連のユーティリティライブラリです。

## 説明

Utf8Utilityは、UTF-8関連処理の実装を詰め合わせたライブラリです。

## インストール

### NuGet（正式リリース版）

```shell
dotnet add package Utf8Utility
```

### Azure Artifacts（開発用ビルド）

```shell
dotnet add package Utf8Utility -s https://pkgs.dev.azure.com/finphie/Main/_packaging/DotNet/nuget/v3/index.json
```

## 使い方

```csharp
using System;
using Utf8Utility;
using Utf8Utility.Text;

// stringまたはUTF-8のバイト配列、ReadOnlySpan{char|byte}を指定できます。
var array = new Utf8Array("abc");

var span = array.AsSpan();

// バイト数
var byteCount = array.ByteCount;

// 文字数
var length1 = array.GetLength();
var length2 = UnicodeUtility.GetLength(span);

// 空かどうか
var isEmpty = array.IsEmpty;

// 空か空白文字列かどうか
var isEmptyOrWhiteSpace1 = array.IsEmptyOrWhiteSpace();
var isEmptyOrWhiteSpace2 = UnicodeUtility.IsEmptyOrWhiteSpace(span);

// Ascii文字列かどうか
var isAscii1 = array.IsAscii();
var isAscii2 = UnicodeUtility.IsAscii(span);

// 内部配列への参照
ref var start = ref array.DangerousGetReference();

// 比較
var compareTo = array.CompareTo(array);
var compare1 = Utf8Array.CompareOrdinal(array, array);
var compare2 = Utf8Array.Compare(array, array, StringComparison.CurrentCulture);
var compare3 = UnicodeUtility.Compare(span, span, StringComparison.CurrentCulture);

var empty = Utf8Array.Empty;
var equals = array.Equals(array);
var hash = array.GetHashCode();
var utf16 = array.ToString();

_ = array.TryFormat(stackalloc char[256], out var charsWritten);
_ = array.TryFormat(stackalloc byte[256], out var bytesWritten);

array.CopyTo(stackalloc byte[256]);
_ = array.TryCopyTo(stackalloc byte[256]);

var chars = array.GetChars(stackalloc char[256]);
_ = array.TryGetChars(stackalloc char[256], out var charsWritten);
```

## サポートフレームワーク

- .NET 9
- .NET 8
- .NET Standard 2.1

## 作者

finphie

## ライセンス

MIT

## クレジット

このプロジェクトでは、次のライブラリ等を使用しています。

### ライブラリ

- [CommunityToolkit.Diagnostics](https://github.com/CommunityToolkit/dotnet)
- [CommunityToolkit.HighPerformance](https://github.com/CommunityToolkit/dotnet)

### テスト

- [Microsoft.Testing.Extensions.CodeCoverage](https://github.com/microsoft/codecoverage)
- [Shouldly](https://github.com/shouldly/shouldly)
- [xunit.v3](https://github.com/xunit/xunit)

### アナライザー

- [DocumentationAnalyzers](https://github.com/DotNetAnalyzers/DocumentationAnalyzers)
- [IDisposableAnalyzers](https://github.com/DotNetAnalyzers/IDisposableAnalyzers)
- [Microsoft.CodeAnalysis.NetAnalyzers](https://github.com/dotnet/roslyn-analyzers)
- [Microsoft.VisualStudio.Threading.Analyzers](https://github.com/Microsoft/vs-threading)
- [Roslynator.Analyzers](https://github.com/dotnet/roslynator)
- [Roslynator.Formatting.Analyzers](https://github.com/dotnet/roslynator)
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

- [PolySharp](https://github.com/Sergio0694/PolySharp)
