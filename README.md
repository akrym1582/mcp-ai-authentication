# mcp-ai-authentication

MCP (Model Context Protocol) ツール呼び出し時の OAuth ベース認証を `Microsoft.Extensions.DependencyInjection` に自然に組み込むための .NET ライブラリです。  
401 (`Unauthorized`) 応答時のチャレンジ処理と再試行を、拡張しやすいハンドラで扱えます。

## 主な機能

- `DelegatingHandler` によるトークン付与
- 401 応答時のチャレンジ通知 (`WWW-Authenticate` 受け取り)
- 認証再試行ポリシーの差し替え
- DI 拡張メソッドでの簡単な組み込み
- MCP ツールを `AIFunction` として登録する拡張

## プロジェクト構成

- `Microsoft.Extensions.AI.Mcp.Authentication`  
  認証ハンドラ本体 (`AuthenticatedHttpMessageHandler`) と関連インターフェイス/オプション
- `Microsoft.Extensions.AI.Mcp.Authentication.Extensions`  
  DI 登録、`HttpClient` 統合、MCP ツール登録拡張
- `Microsoft.Extensions.AI.Mcp.Authentication.Tests`  
  xUnit v3 による動作確認テスト

## クイックスタート

`IMcpAuthenticationHandler` を実装し、`HttpClient` に認証ハンドラを追加します。

```csharp
using Microsoft.Extensions.AI.Mcp.Authentication;
using Microsoft.Extensions.AI.Mcp.Authentication.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IMcpAuthenticationHandler, MyAuthenticationHandler>();
services.AddMcpAuthentication(options =>
{
    options.RetryOnUnauthorized = true;
    options.MaxRetryCount = 1;
});

services.AddHttpClient("mcp")
    .AddMcpAuthenticationHandler();
```

`IMcpAuthenticationHandler` では以下を実装します。

- `GetAccessTokenAsync`  
  各リクエスト送信前にアクセストークンを返す
- `HandleAuthenticationChallengeAsync`  
  401 応答時にチャレンジ情報を受け取り、必要であればトークン更新などを実施する

## MCP ツールの登録

MCP ツールを `AIFunction` として DI 経由で公開できます。

```csharp
services.AddMcpTools(builder =>
{
    builder.AddTool("ping", "returns pong", _ => new ValueTask<object?>("pong"));
});
```

`IEnumerable<AIFunction>` または `IReadOnlyList<AIFunction>` を解決して利用します。

## 開発

### 要件

- .NET SDK 10

### ビルド

```bash
dotnet build McpAiAuthentication.slnx
```

### テスト

```bash
dotnet run --project Microsoft.Extensions.AI.Mcp.Authentication.Tests/Microsoft.Extensions.AI.Mcp.Authentication.Tests.csproj
```
