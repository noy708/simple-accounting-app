# Design Document

## Overview

超シンプルな会計アプリケーションの設計。取引入力機能に特化し、.NET バックエンドAPIとNext.jsフロントエンドで構成するフルスタックアプリケーション。Material-UIを使用してモダンで使いやすいUIを提供する。

## Architecture

### アーキテクチャパターン
- **クライアント・サーバーアーキテクチャ**
- **RESTful API設計**
- **レイヤードアーキテクチャ（バックエンド）**
- **コンポーネントベース設計（フロントエンド）**

### 技術スタック
- **バックエンド**: .NET 6/8, ASP.NET Core Web API
- **データベース**: Entity Framework Core + SQLite（開発用）
- **PDF生成**: Microsoft Playwright（HTMLからPDF生成）
- **フロントエンド**: Next.js 14, React 18, TypeScript
- **UI フレームワーク**: Material-UI (MUI) v5
- **HTTP クライアント**: Axios
- **開発ツール**: Visual Studio Code, .NET CLI, npm/yarn

### プロジェクト構造
```
simple-accounting-app/
├── backend/
│   ├── SimpleAccounting.API/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Data/
│   │   └── Program.cs
│   └── SimpleAccounting.sln
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   └── types/
│   ├── package.json
│   └── next.config.js
└── README.md
```

## Components and Interfaces

### Backend Components (.NET)

#### 1. Transaction Model
```csharp
public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum TransactionType
{
    Income,
    Expense
}
```

#### 2. Transaction Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
    
    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(CreateTransactionDto dto)
    
    [HttpGet("balance")]
    public async Task<ActionResult<decimal>> GetBalance()
    
    [HttpGet("pdf")]
    public async Task<IActionResult> DownloadPdf()
}
```

#### 3. Transaction Service
```csharp
public interface ITransactionService
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
    Task<Transaction> CreateTransactionAsync(CreateTransactionDto dto);
    Task<decimal> GetBalanceAsync();
}
```

#### 4. PDF Service
```csharp
public interface IPdfService
{
    Task<byte[]> GenerateTransactionsPdfAsync();
}

public class PdfService : IPdfService
{
    // iTextSharpまたはQuestPDFを使用してPDF生成
}
```

#### 4. Data Context
```csharp
public class AccountingDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }
}
```

### Frontend Components (Next.js + Material-UI)

#### 1. Transaction Form Component
```typescript
interface TransactionFormProps {
  onSubmit: (transaction: CreateTransactionDto) => void;
}

export const TransactionForm: React.FC<TransactionFormProps>
```

#### 2. Transaction List Component
```typescript
interface TransactionListProps {
  transactions: Transaction[];
}

export const TransactionList: React.FC<TransactionListProps>
```

#### 3. Balance Display Component
```typescript
interface BalanceDisplayProps {
  balance: number;
}

export const BalanceDisplay: React.FC<BalanceDisplayProps>
```

#### 4. Main Dashboard Page
```typescript
export default function Dashboard(): JSX.Element
```

#### 5. PDF Download Component
```typescript
interface PdfDownloadProps {
  onDownload: () => void;
  isLoading: boolean;
}

export const PdfDownloadButton: React.FC<PdfDownloadProps>
```

## Data Models

### Backend Models (.NET)

#### Transaction Entity
```csharp
public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### DTOs
```csharp
public class CreateTransactionDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
}

public class TransactionResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string Date { get; set; }
}
```

### Frontend Types (TypeScript)

#### Transaction Types
```typescript
export interface Transaction {
  id: number;
  amount: number;
  description: string;
  type: 'Income' | 'Expense';
  date: string;
  createdAt: string;
}

export interface CreateTransactionDto {
  amount: number;
  description: string;
  type: 'Income' | 'Expense';
  date: string;
}
```

### Database Schema (SQLite)
```sql
CREATE TABLE Transactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    Type INTEGER NOT NULL,
    Date DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

## Error Handling

### Backend Validation (.NET)
```csharp
public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Date).NotEmpty();
    }
}
```

### Frontend Validation (React Hook Form + Yup)
```typescript
const transactionSchema = yup.object({
  amount: yup.number().positive().required(),
  description: yup.string().required().max(200),
  type: yup.string().oneOf(['Income', 'Expense']).required(),
  date: yup.date().required()
});
```

### エラー表示戦略
- **バックエンド**: 統一されたエラーレスポンス形式
- **フロントエンド**: Material-UIのAlert/Snackbarコンポーネント
- **バリデーション**: リアルタイムフィールドバリデーション
- **API エラー**: Try-catchでハンドリングし、ユーザーフレンドリーなメッセージ表示

## Testing Strategy

### 最小限のテスト戦略

#### Backend Testing (.NET)
1. **単体テスト**: xUnit + Moq
   - TransactionService のビジネスロジック
   - Controller のエンドポイント
   
2. **統合テスト**: ASP.NET Core Test Host
   - API エンドポイントの動作確認
   - データベース操作の確認

#### Frontend Testing (Next.js)
1. **コンポーネントテスト**: Jest + React Testing Library
   - TransactionForm の動作
   - TransactionList の表示
   
2. **手動テスト**:
   - ブラウザでの動作確認
   - レスポンシブデザインの確認

### テストケース（最小限）
- **API**: 取引作成、取引一覧取得、残高計算
- **UI**: フォーム送信、データ表示、エラーハンドリング
- **統合**: フロントエンド・バックエンド間の通信

## UI/UX Design (Material-UI)

### レイアウト構成
- **AppBar**: アプリタイトルと現在残高表示
- **Container**: メインコンテンツエリア
- **Grid システム**: レスポンシブレイアウト
- **Card コンポーネント**: 取引入力フォームと一覧表示

### Material-UI コンポーネント使用
- **TextField**: 入力フィールド
- **Button**: アクションボタン
- **Select**: 取引タイプ選択
- **Table**: 取引一覧表示
- **Chip**: 取引タイプ表示
- **Alert/Snackbar**: エラー・成功メッセージ

### テーマ設定
```typescript
const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    success: {
      main: '#2e7d32', // 収入
    },
    error: {
      main: '#d32f2f', // 支出
    },
  },
});
```

### レスポンシブデザイン
- Material-UIのGrid システムを活用
- xs, sm, md, lg ブレークポイント対応
- モバイルファースト設計

## PDF Generation Design

### PDF生成アーキテクチャ
- **ライブラリ選択**: Microsoft Playwright（HTMLからPDF生成）
- **生成方式**: HTMLテンプレートを作成し、PlaywrightでPDF変換
- **ファイル配信**: HTTP応答でContent-Dispositionヘッダーを使用してダウンロード
- **テンプレート**: 文字列テンプレートでHTML生成
- **ブラウザエンジン**: Chromiumを使用してPDF生成

### PDF生成フロー
```csharp
public class PdfService : IPdfService
{
    public async Task<byte[]> GenerateTransactionsPdfAsync()
    {
        // 1. 取引データを取得
        // 2. HTMLテンプレートに取引データを埋め込み
        // 3. PlaywrightでHTMLをPDF変換
        // 4. バイナリデータを返却
    }
}
```

### Playwright設定
- **ブラウザ**: Chromium（軽量で高速）
- **ヘッドレスモード**: サーバー環境での実行
- **セキュリティ**: サンドボックス無効化（Docker環境対応）
- **メモリ管理**: ブラウザインスタンスの適切な破棄

### HTMLテンプレート設計
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        /* PDF用のCSSスタイル */
        @font-face {
            font-family: 'IPAexMincho';
            src: url('https://moji.or.jp/wp-content/ipafont/IPAexfont/ipaexm.ttf') format('truetype');
        }
        body { font-family: 'IPAexMincho', serif; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        .income { color: green; }
        .expense { color: red; }
    </style>
</head>
<body>
    <h1>仕訳帳</h1>
    <p>生成日時: {{GeneratedDate}}</p>
    <table>
        <thead>
            <tr><th>日付</th><th>説明</th><th>金額</th><th>取引タイプ</th></tr>
        </thead>
        <tbody>
            {{#each Transactions}}
            <tr>
                <td>{{Date}}</td>
                <td>{{Description}}</td>
                <td class="{{TypeClass}}">{{Amount}}</td>
                <td>{{Type}}</td>
            </tr>
            {{/each}}
        </tbody>
    </table>
    <p><strong>合計残高: {{Balance}}</strong></p>
</body>
</html>
```

### PDF設定
- **ページサイズ**: A4
- **マージン**: 上下左右 20mm
- **フォント**: IPAex明朝（日本語明朝体フォント）
- **印刷最適化**: CSS @media print ルール適用

### エラーハンドリング
- PDF生成失敗時は500エラーとエラーメッセージを返却
- 取引データなしの場合は空のテーブルと「データなし」メッセージを表示
- Playwrightブラウザ起動失敗時の適切なエラーハンドリング
- ブラウザインスタンスのリソースリーク防止

## Performance Considerations

### 最適化戦略
- **Next.js**: 自動コード分割とSSR
- **React**: useMemo, useCallback でレンダリング最適化
- **API**: ページネーション実装（将来的に）
- **データベース**: インデックス設定
- **PDF生成**: メモリ効率を考慮し、大量データの場合はストリーミング処理

### 開発効率重視
- 複雑な最適化は後回し
- 基本的な機能の迅速な実装を優先
- SQLite使用で設定を最小限に
- Microsoft Playwright使用でHTMLベースの柔軟なPDF生成
- Playwrightの安定性とパフォーマンスを活用