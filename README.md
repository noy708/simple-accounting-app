# Simple Accounting App

超シンプルな会計アプリケーション - 日々の収入・支出を簡単に記録できるWebアプリケーション

## 概要

このアプリケーションは、個人や小規模事業者が日々の取引を簡単に記録し、残高を確認できるシンプルな会計システムです。

### 主な機能

- ✅ 取引の入力（収入・支出）
- ✅ 取引履歴の表示
- ✅ 現在残高の表示
- ✅ レスポンシブデザイン
- ✅ リアルタイム更新

## 技術スタック

### バックエンド
- .NET 8 Web API
- Entity Framework Core
- SQLite データベース
- FluentValidation
- Swagger/OpenAPI

### フロントエンド
- Next.js 14
- React 18
- TypeScript
- Material-UI (MUI) v5
- Axios

## セットアップ手順

### 前提条件

- .NET 8 SDK
- Node.js 18+ 
- npm または yarn

### 1. リポジトリのクローン

```bash
git clone <repository-url>
cd simple-accounting-app
```

### 2. バックエンドのセットアップ

```bash
cd SimpleAccounting.API

# パッケージの復元
dotnet restore

# データベースの作成（初回のみ）
dotnet ef database update

# アプリケーションの起動
dotnet run
```

バックエンドは `http://localhost:5212` で起動します。
Swagger UI は `http://localhost:5212/swagger` でアクセスできます。

### 3. フロントエンドのセットアップ

```bash
cd frontend

# 依存関係のインストール
npm install

# 開発サーバーの起動
npm run dev
```

フロントエンドは `http://localhost:3000` で起動します。

## 開発環境での実行

### バックエンドとフロントエンドを同時に起動

1. ターミナル1でバックエンドを起動:
```bash
cd SimpleAccounting.API
dotnet run
```

2. ターミナル2でフロントエンドを起動:
```bash
cd frontend
npm run dev
```

3. ブラウザで `http://localhost:3000` にアクセス

## API エンドポイント

### 取引関連

- `GET /api/transactions` - 全取引の取得
- `POST /api/transactions` - 新しい取引の作成
- `GET /api/transactions/balance` - 現在残高の取得

### リクエスト例

#### 取引の作成
```json
POST /api/transactions
{
  "amount": 1000.50,
  "description": "給与",
  "type": 0,  // 0: Income, 1: Expense
  "date": "2025-01-06T00:00:00"
}
```

#### レスポンス例
```json
{
  "id": 1,
  "amount": 1000.50,
  "description": "給与",
  "type": 0,
  "date": "2025-01-06T00:00:00",
  "createdAt": "2025-01-06T10:30:00"
}
```

## テスト

### バックエンドテスト

```bash
cd SimpleAccounting.Tests
dotnet test
```

### フロントエンドテスト

```bash
cd frontend
npm test
```

### 統合テスト

```bash
# プロジェクトルートで実行
./test-integration.sh
```

## 本番環境デプロイ

### Docker を使用したデプロイ（推奨）

1. Docker Compose を使用した簡単デプロイ:
```bash
# アプリケーション全体を起動
docker-compose up -d

# ログの確認
docker-compose logs -f
```

2. 個別にビルドする場合:
```bash
# バックエンドのビルドと起動
cd SimpleAccounting.API
docker build -t accounting-api .
docker run -d -p 5212:80 -v accounting_data:/app/data accounting-api

# フロントエンドのビルドと起動
cd frontend
docker build -t accounting-frontend .
docker run -d -p 3000:3000 accounting-frontend
```

### 従来のデプロイ方法

#### バックエンド

1. 本番用設定ファイルの更新:
   - `appsettings.Production.json` でデータベース接続文字列とCORS設定を更新

2. アプリケーションのビルド:
```bash
dotnet publish -c Release -o ./publish
```

3. 本番サーバーでの実行:
```bash
cd publish
dotnet SimpleAccounting.API.dll --environment=Production
```

#### フロントエンド

1. 環境変数の設定:
   - `.env.production` でAPI URLを本番環境のものに更新

2. アプリケーションのビルド:
```bash
npm run build
```

3. 静的ファイルのデプロイ:
   - `out/` フォルダの内容をWebサーバーにデプロイ

## 設定

### 環境変数

#### バックエンド
- `ASPNETCORE_ENVIRONMENT`: 実行環境 (Development/Production)
- `ConnectionStrings__DefaultConnection`: データベース接続文字列

#### フロントエンド
- `NEXT_PUBLIC_API_URL`: バックエンドAPIのURL

### CORS設定

本番環境では `appsettings.Production.json` でCORS設定を適切に設定してください:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://your-frontend-domain.com"
    ]
  }
}
```

## トラブルシューティング

### よくある問題

1. **CORS エラー**
   - バックエンドのCORS設定を確認
   - フロントエンドのAPIエンドポイントURLを確認

2. **データベース接続エラー**
   - SQLiteファイルの権限を確認
   - 接続文字列が正しいか確認

3. **ポート競合**
   - 他のアプリケーションが同じポートを使用していないか確認
   - `launchSettings.json` でポート設定を変更可能

### ログの確認

- バックエンド: コンソール出力またはログファイル
- フロントエンド: ブラウザの開発者ツール

## ライセンス

MIT License

## 貢献

プルリクエストやイシューの報告を歓迎します。

## 追加ドキュメント

- [使用方法ガイド](USAGE.md) - アプリケーションの詳しい使い方
- [デプロイメントガイド](DEPLOYMENT.md) - 本番環境への詳細なデプロイ手順

## サポート

問題が発生した場合は、GitHubのIssuesページで報告してください。