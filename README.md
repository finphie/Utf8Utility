# Utf8Utility

[![Build(.NET)](https://github.com/finphie/Utf8Utility/actions/workflows/build-dotnet.yml/badge.svg)](https://github.com/finphie/Utf8Utility/actions/workflows/build-dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/Utf8Utility?color=0078d4&label=NuGet)](https://www.nuget.org/packages/Utf8Utility/)
[![Utf8Utility package in DotNet feed in Azure Artifacts](https://feeds.dev.azure.com/finphie/7af9aa4d-c550-43af-87a5-01539b2d9934/_apis/public/Packaging/Feeds/18cbb017-6f1d-41eb-b9a5-a6dbf411e3f7/Packages/72c69351-0c11-40f2-8853-5712bf32468d/Badge)](https://dev.azure.com/finphie/Main/_packaging?_a=package&feed=18cbb017-6f1d-41eb-b9a5-a6dbf411e3f7&package=72c69351-0c11-40f2-8853-5712bf32468d&preferRelease=true)

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
using Utf8Utility;

var s = new Utf8String("abc");
var dict = new Utf8StringDictionary<int>();

dict.TryAdd(s, 1);
dict.TryGetValue(s, out var result);
```

## サポートフレームワーク

- .NET 6
- .NET Standard 2.0

## 作者

finphie

## ライセンス

MIT

## クレジット

このプロジェクトでは、下記のライブラリ等を使用しています。

### ライブラリ

- Microsoft.Toolkit.HighPerformance  
<https://github.com/windows-toolkit/WindowsCommunityToolkit>  
(c) .NET Foundation and Contributors.