# componentize-dotnet_example2

## 注意！まだ未整理

## 概要
componentize-dotnet の Creating a WASI 0.2 component, including WIT support を試す。

.NETでWebAssemblyの最新仕様「WASI Preview 2」対応コンポーネントを作れる「componentize-dotnet」、Bytecode Allianceがオープンソースでリリース  
https://www.publickey1.jp/blog/24/netwebassemblywasi_preview_2componentize-dotnetbytecode_alliance.html  

componentize-dotnet  
https://github.com/bytecodealliance/componentize-dotnet/tree/main  

## 手順
* WIT ファイルで add 関数を持つ calculator を定義　→　calculator.wit
* 上記の定義を参照し、add 関数を呼び出す wasm を作成　→　console.wasm
* 上記の定義を参照し、add 関数を実装した wasm を作成　→　lib.wasm
* console.wasm と lib.wasm を結合した wasm を作成　→　composed.wasm
* composed.wasm を実行　→　123 + 456 = 579 が出力される

## 環境
```
> dotnet --info
.NET SDK:
 Version:           8.0.401   
 Commit:            811edcc344
 Workload version:  8.0.400-manifests.56cd0383
 MSBuild version:   17.11.4+37eb419ad

ランタイム環境:
 OS Name:     Windows
 OS Version:  10.0.19045
 OS Platform: Windows
 RID:         win-x64
```

```
> .\tools\wasmtime-v24.0.0-x86_64-windows\wasmtime.exe --version
wasmtime-cli 24.0.0 (6fc3d274c 2024-08-20)
```

```
> .\tools\wasm-tools-1.216.0-x86_64-windows\wasm-tools.exe --version
wasm-tools 1.216.0 (28c8962b1 2024-08-22)
```

## 実行結果
```
> .\tools\wasmtime-v24.0.0-x86_64-windows\wasmtime.exe .\outnative\composed.wasm
TODO: cabi_post_example:calculator/operations#add
123 + 456 = 579
```
※この TODO は何だ？。。。

## 詳細

```
dotnet new console -n WitExample.Console
```

```
dotnet new nugetconfig
```

### `nuget.config` の `<clear />` の後ろに以下を追加
```
    <add key="dotnet-experimental" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-experimental/nuget/v3/index.json" />
    <add key="nuget" value="https://api.nuget.org/v3/index.json" />
```

### パッケージ追加
```
dotnet add WitExample.Console package BytecodeAlliance.Componentize.DotNet.Wasm.SDK --prerelease
```

### WIT ファイルの作成

calculator.wit
```
package example:calculator;

interface operations {
  add: func(left: s32, right: s32) -> s32;
}

world computer {
  export operations;
}

world hostapp {
  import operations;
}
```

### プロジェクトに作成した WIT ファイルを追加

WitExample.Console\WitExample.Console.csproj
```xml
  <ItemGroup>
    <Wit Include="..\calculator.wit" World="hostapp" />
  </ItemGroup>
```

### プログラムで WIT ファイルを利用

WitExample.Console\Program.cs
```cs
using HostappWorld.wit.imports.example.calculator;

var left = 123;
var right = 456;
var result = OperationsInterop.Add(left, right);
Console.WriteLine($"{left} + {right} = {result}");
```

### `WitExample.Console\WitExample.Console.csproj` の `<PropertyGroup>` に以下を追加

```xml
    <RuntimeIdentifier>wasi-wasm</RuntimeIdentifier>
    <UseAppHost>false</UseAppHost>
    <PublishTrimmed>true</PublishTrimmed>
    <InvariantGlobalization>true</InvariantGlobalization>

    <AssemblyName>console</AssemblyName>
```
※AssemblyName　→　kebab case (単語がハイフンで区切られている形式（例: wit-example-lib.wasm）) でないとエラーになる

### ビルド
```
dotnet build WitExample.Console -o out
```

実行結果
```
> dotnet build WitExample.Console -o out
  復元対象のプロジェクトを決定しています...
  復元対象のすべてのプロジェクトは最新です。
  Executing wit-bindgen...
  Generating "obj\\Debug\\net8.0\\wasi-wasm\\wit_bindgen\\Hostapp.cs"
  Generating "obj\\Debug\\net8.0\\wasi-wasm\\wit_bindgen\\HostappWorld.wit.imports.example.calculator.OperationsInterop.cs"
  Generating "obj\\Debug\\net8.0\\wasi-wasm\\wit_bindgen\\HostappWorld_component_type.wit"
  Generating "obj\\Debug\\net8.0\\wasi-wasm\\wit_bindgen\\HostappWorld_wasm_import_linkage_attribute.cs"
CSC : warning CS8784: ジェネレーター 'VtableIndexStubGenerator' を初期化できませんでした。出力には寄与しません。結果として、コンパイル エラーが発生する可能性があります。例外の型: 'TypeLoadException'。メッセージ: 'Could not load type 'Microsoft.Intero
p.MarshallingGeneratorFactoryKey`1' from assembly 'Microsoft.Interop.SourceGeneration, Version=9.0.10.42901, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'.'。 [D:\Projects\componentize-dotnet_example2\WitExample.Console\WitExample.Console.csproj]
CSC : warning CS8784: ジェネレーター 'LibraryImportGenerator' を初期化できませんでした。出力には寄与しません。結果として、コンパイル エラーが発生する可能性があります。例外の型: 'TypeLoadException'。メッセージ: 'Could not load type 'Microsoft.Interop.
MarshallingGeneratorFactoryKey`1' from assembly 'Microsoft.Interop.SourceGeneration, Version=9.0.10.42901, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'.'。 [D:\Projects\componentize-dotnet_example2\WitExample.Console\WitExample.Console.csproj]
  WitExample.Console -> D:\Projects\componentize-dotnet_example2\out\console.dll
  Generating native code
  LLVM compilation to IR finished in 0.73 seconds
  LLVM generation of bitcode finished in 0.13 seconds
  Object writing finished in 0.12 seconds, allocated 29.61 MB
  Emit on build WitExample.Console 

ビルドに成功しました。

CSC : warning CS8784: ジェネレーター 'VtableIndexStubGenerator' を初期化できませんでした。出力には寄与しません。結果として、コンパイル エラーが発生する可能性があります。例外の型: 'TypeLoadException'。メッセージ: 'Could not load type 'Microsoft.Intero
p.MarshallingGeneratorFactoryKey`1' from assembly 'Microsoft.Interop.SourceGeneration, Version=9.0.10.42901, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'.'。 [D:\Projects\componentize-dotnet_example2\WitExample.Console\WitExample.Console.csproj]
CSC : warning CS8784: ジェネレーター 'LibraryImportGenerator' を初期化できませんでした。出力には寄与しません。結果として、コンパイル エラーが発生する可能性があります。例外の型: 'TypeLoadException'。メッセージ: 'Could not load type 'Microsoft.Interop.
MarshallingGeneratorFactoryKey`1' from assembly 'Microsoft.Interop.SourceGeneration, Version=9.0.10.42901, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'.'。 [D:\Projects\componentize-dotnet_example2\WitExample.Console\WitExample.Console.csproj]
    2 個の警告
    0 エラー

経過時間 00:00:05.49
```

`out` と `outnative` フォルダが作成され `outnative` に `console.wasm` が作成される

試しにこれ単体で実行してみる

wasmtime-v24.0.0-x86_64-windows をダウンロード
```
Invoke-WebRequest -Uri "https://github.com/bytecodealliance/wasmtime/releases/download/v24.0.0/wasmtime-v24.0.0-x86_64-windows.zip" -OutFile "$pwd\wasmtime.zip"; Expand-Archive -Path "$pwd\wasmtime.zip" -DestinationPath "$pwd\tools"; Remove-Item "$pwd\wasmtime.zip"
```

```
.\tools\wasmtime-v24.0.0-x86_64-windows\wasmtime.exe outnative\console.wasm
```

実行結果  
当然エラー
```
> .\tools\wasmtime-v24.0.0-x86_64-windows\wasmtime.exe outnative\console.wasm
Error: failed to run main module `outnative\console.wasm`

Caused by:
    0: component imports instance `example:calculator/operations`, but a matching implementation was not found in the linker
    1: instance export `add` has the wrong type
    2: function implementation is missing
```



### WIT ファイルの実装を作成

```
dotnet new classlib -n WitExample.Lib
```

### パッケージ追加
```
dotnet add WitExample.Lib package BytecodeAlliance.Componentize.DotNet.Wasm.SDK --prerelease
```

### `WitExample.Lib\WitExample.Lib.csproj` の `<PropertyGroup>` に以下を追加

```xml
    <RuntimeIdentifier>wasi-wasm</RuntimeIdentifier>
    <UseAppHost>false</UseAppHost>
    <PublishTrimmed>true</PublishTrimmed>
    <InvariantGlobalization>true</InvariantGlobalization>

    <AssemblyName>lib</AssemblyName>
```
※AssemblyName　→　kebab case (単語がハイフンで区切られている形式（例: wit-example-lib.wasm）) でないとエラーになる

WitExample.Lib\WitExample.Lib.csproj
```xml
  <ItemGroup>
    <Wit Include="..\calculator.wit" World="computer" />
  </ItemGroup>
```
※`World` が `computer` になっていることに注意


### ビルド
```
dotnet build WitExample.Lib -o out
```

実行結果
```
> dotnet build WitExample.Lib -o out
  復元対象のプロジェクトを決定しています...
  復元対象のすべてのプロジェクトは最新です。
  Executing wit-bindgen...
  Generating "obj\\Debug\\net8.0\\wasi-wasm\\wit_bindgen\\Computer.cs"
  Generating "obj\\Debug\\net8.0\\wasi-wasm\\wit_bindgen\\ComputerWorld.wit.exports.example.calculator.IOperations.cs"
  Generating "obj\\Debug\\net8.0\\wasi-wasm\\wit_bindgen\\ComputerWorld.wit.exports.example.calculator.OperationsInterop.cs"
  Generating "obj\\Debug\\net8.0\\wasi-wasm\\wit_bindgen\\ComputerWorld_component_type.wit"
  Generating "obj\\Debug\\net8.0\\wasi-wasm\\wit_bindgen\\ComputerWorld_wasm_import_linkage_attribute.cs"
CSC : warning CS8784: ジェネレーター 'VtableIndexStubGenerator' を初期化できませんでした。出力には寄与しません。結果として、コンパイル エラーが発生する可能性があります。例外の型: 'TypeLoadException'。メッセージ: 'Could not load type 'Microsoft.Intero
p.MarshallingGeneratorFactoryKey`1' from assembly 'Microsoft.Interop.SourceGeneration, Version=9.0.10.42901, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'.'。 [D:\Projects\componentize-dotnet_example2\WitExample.Lib\WitExample.Lib.csproj]
CSC : warning CS8784: ジェネレーター 'LibraryImportGenerator' を初期化できませんでした。出力には寄与しません。結果として、コンパイル エラーが発生する可能性があります。例外の型: 'TypeLoadException'。メッセージ: 'Could not load type 'Microsoft.Interop.
MarshallingGeneratorFactoryKey`1' from assembly 'Microsoft.Interop.SourceGeneration, Version=9.0.10.42901, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'.'。 [D:\Projects\componentize-dotnet_example2\WitExample.Lib\WitExample.Lib.csproj]
  WitExample.Lib -> D:\Projects\componentize-dotnet_example2\out\lib.dll
  Generating native code
  LLVM compilation to IR finished in 0.76 seconds
  LLVM generation of bitcode finished in 0.13 seconds
  Object writing finished in 0.09 seconds, allocated 29.23 MB
  Emit on build WitExample.Lib 

ビルドに成功しました。

CSC : warning CS8784: ジェネレーター 'VtableIndexStubGenerator' を初期化できませんでした。出力には寄与しません。結果として、コンパイル エラーが発生する可能性があります。例外の型: 'TypeLoadException'。メッセージ: 'Could not load type 'Microsoft.Intero
p.MarshallingGeneratorFactoryKey`1' from assembly 'Microsoft.Interop.SourceGeneration, Version=9.0.10.42901, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'.'。 [D:\Projects\componentize-dotnet_example2\WitExample.Lib\WitExample.Lib.csproj]
CSC : warning CS8784: ジェネレーター 'LibraryImportGenerator' を初期化できませんでした。出力には寄与しません。結果として、コンパイル エラーが発生する可能性があります。例外の型: 'TypeLoadException'。メッセージ: 'Could not load type 'Microsoft.Interop.
MarshallingGeneratorFactoryKey`1' from assembly 'Microsoft.Interop.SourceGeneration, Version=9.0.10.42901, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'.'。 [D:\Projects\componentize-dotnet_example2\WitExample.Lib\WitExample.Lib.csproj]
    2 個の警告
    0 エラー

経過時間 00:00:04.42
```

### console.wasm と lib.wasm を組合せて composed.wasm を作成
```
.\tools\wasm-tools-1.216.0-x86_64-windows\wasm-tools.exe compose -o .\outnative\composed.wasm .\outnative\console.wasm -d .\outnative\lib.wasm
```

※ wasm-tools
```
Invoke-WebRequest -Uri "https://github.com/bytecodealliance/wasm-tools/releases/download/v1.216.0/wasm-tools-1.216.0-x86_64-windows.zip" -OutFile "$pwd\wasm-tools.zip"; Expand-Archive -Path "$pwd\wasm-tools.zip" -DestinationPath "$pwd\tools"; Remove-Item "$pwd\wasm-tools.zip"
```

実行結果
```
> .\tools\wasm-tools-1.216.0-x86_64-windows\wasm-tools.exe compose -o .\outnative\composed.wasm .\outnative\console.wasm -d .\outnative\lib.wasm
WARNING: `wasm-tools compose` has been deprecated.

Please use `wac` instead. You can find more information about `wac` at https://github.com/bytecodealliance/wac.
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/environment@0.2.0` will be imported because a dependency named `wasi:cli/environment@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/exit@0.2.0` will be imported because a dependency named `wasi:cli/exit@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:io/error@0.2.0` will be imported because a dependency named `wasi:io/error@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:io/poll@0.2.0` will be imported because a dependency named `wasi:io/poll@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:io/streams@0.2.0` will be imported because a dependency named `wasi:io/streams@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:io/streams@0.2.0` will be imported because a dependency named `wasi:io/streams@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/stdin@0.2.0` will be imported because a dependency named `wasi:cli/stdin@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/stdin@0.2.0` will be imported because a dependency named `wasi:cli/stdin@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/stdout@0.2.0` will be imported because a dependency named `wasi:cli/stdout@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/stderr@0.2.0` will be imported because a dependency named `wasi:cli/stderr@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/terminal-input@0.2.0` will be imported because a dependency named `wasi:cli/terminal-input@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/terminal-input@0.2.0` will be imported because a dependency named `wasi:cli/terminal-input@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/terminal-output@0.2.0` will be imported because a dependency named `wasi:cli/terminal-output@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/terminal-stdin@0.2.0` will be imported because a dependency named `wasi:cli/terminal-stdin@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/terminal-stdout@0.2.0` will be imported because a dependency named `wasi:cli/terminal-stdout@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:cli/terminal-stderr@0.2.0` will be imported because a dependency named `wasi:cli/terminal-stderr@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:clocks/monotonic-clock@0.2.0` will be imported because a dependency named `wasi:clocks/monotonic-clock@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:clocks/wall-clock@0.2.0` will be imported because a dependency named `wasi:clocks/wall-clock@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:filesystem/types@0.2.0` will be imported because a dependency named `wasi:filesystem/types@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:filesystem/preopens@0.2.0` will be imported because a dependency named `wasi:filesystem/preopens@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:sockets/network@0.2.0` will be imported because a dependency named `wasi:sockets/network@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:sockets/udp@0.2.0` will be imported because a dependency named `wasi:sockets/udp@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:sockets/tcp@0.2.0` will be imported because a dependency named `wasi:sockets/tcp@0.2.0` could not be found
[2024-09-09T04:15:33Z WARN ] instance `wasi:random/random@0.2.0` will be imported because a dependency named `wasi:random/random@0.2.0` could not be found
composed component `.\outnative\composed.wasm`
```

### composed.wasm を実行
```
.\tools\wasmtime-v24.0.0-x86_64-windows\wasmtime.exe .\outnative\composed.wasm
```
実行結果
```
> .\tools\wasmtime-v24.0.0-x86_64-windows\wasmtime.exe .\outnative\composed.wasm
TODO: cabi_post_example:calculator/operations#add
123 + 456 = 579
```
※この TODO は何だ？。。。

## トラブルシューティング
```
> .\tools\wasm-tools-1.216.0-x86_64-windows\wasm-tools.exe compose -o .\outnative\composed.wasm .\outnative\WitExample.Console.wasm -d outnative\WitExample.Lib.wasm
WARNING: wasm-tools compose has been deprecated.

Please use wac instead. You can find more information about wac at https://github.com/bytecodealliance/wac.
error: WitExample.Lib is not in kebab case (at offset 0x0)
```

> WARNING: wasm-tools compose has been deprecated.

wasm-tools compose が非推奨になっており、 wac という別のツールを使えとのこと  
とりあえず無視して wasm-tools compose で出来た。

> error: WitExample.Lib is not in kebab case (at offset 0x0)

wasm ファイル名が kebab case (単語がハイフンで区切られている形式（例: wit-example-lib.wasm）) でないエラー  
ファイル名を kebab case に変更する。

## メモ

```
dotnet build WitExample.Console -o out
dotnet build WitExample.Lib -o out
.\tools\wasm-tools-1.216.0-x86_64-windows\wasm-tools.exe compose -o .\outnative\composed.wasm .\outnative\console.wasm -d .\outnative\lib.wasm
```

```
.\tools\wasmtime-v24.0.0-x86_64-windows\wasmtime.exe .\outnative\composed.wasm
```