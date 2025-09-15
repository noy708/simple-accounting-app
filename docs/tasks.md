# Implementation Plan

- [x] 1. プロジェクト構造とセットアップ
  - .NET Web APIプロジェクトとNext.jsプロジェクトの初期化
  - 必要なNuGetパッケージとnpmパッケージのインストール
  - 基本的なプロジェクト構造の作成
  - _Requirements: 4.1, 4.2_

- [x] 2. バックエンド基盤の実装
- [x] 2.1 データモデルとDbContextの作成
  - Transactionエンティティクラスの実装
  - TransactionTypeエニューム定義
  - AccountingDbContextクラスの作成
  - Entity Framework設定とマイグレーション
  - _Requirements: 1.1, 2.3, 3.4_

- [x] 2.2 DTOクラスとバリデーションの実装
  - CreateTransactionDtoクラスの作成
  - TransactionResponseDtoクラスの作成
  - FluentValidationを使用したバリデーションルールの実装
  - _Requirements: 1.3, 1.4_

- [x] 2.3 TransactionServiceの実装
  - ITransactionServiceインターフェースの定義
  - TransactionServiceクラスの実装（CRUD操作）
  - 残高計算ロジックの実装
  - _Requirements: 1.1, 1.2, 2.1, 2.2, 3.1, 3.4_

- [x] 2.4 TransactionsControllerの実装
  - GET /api/transactions エンドポイント（取引一覧取得）
  - POST /api/transactions エンドポイント（取引作成）
  - GET /api/transactions/balance エンドポイント（残高取得）
  - エラーハンドリングとレスポンス形式の統一
  - _Requirements: 1.1, 1.2, 2.1, 2.2, 3.1_

- [x] 3. フロントエンド基盤の実装
- [x] 3.1 Next.jsプロジェクトセットアップとMaterial-UI設定
  - TypeScript設定とMaterial-UIテーマの作成
  - レイアウトコンポーネントの基本構造
  - APIクライアント（Axios）の設定
  - _Requirements: 4.1, 4.2, 4.3_

- [x] 3.2 TypeScript型定義とAPIサービスの実装
  - Transaction型とCreateTransactionDto型の定義
  - APIサービスクラスの実装（取引CRUD、残高取得）
  - エラーハンドリング用のユーティリティ関数
  - _Requirements: 1.1, 1.2, 2.1, 3.1_

- [x] 4. UIコンポーネントの実装
- [x] 4.1 BalanceDisplayコンポーネントの実装
  - 残高表示用のMaterial-UIコンポーネント
  - プラス/マイナスに応じた色分け表示
  - リアルタイム残高更新機能
  - _Requirements: 3.1, 3.2, 3.3, 3.4_

- [x] 4.2 TransactionFormコンポーネントの実装
  - Material-UIのTextField、Select、DatePickerを使用
  - React Hook Formとyupを使用したフォームバリデーション
  - 送信後のフォームクリア機能
  - エラー表示とローディング状態の管理
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 4.4_

- [x] 4.3 TransactionListコンポーネントの実装
  - Material-UIのTableコンポーネントを使用した取引一覧表示
  - 日付順（新しい順）でのソート表示
  - 収入/支出の色分け表示（Chipコンポーネント使用）
  - 空の状態（取引なし）の表示
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

- [x] 5. メインページの統合
- [x] 5.1 Dashboardページの実装
  - AppBarコンポーネントでヘッダー作成
  - GridレイアウトでBalanceDisplay、TransactionForm、TransactionListを配置
  - レスポンシブデザインの実装
  - _Requirements: 4.1, 4.2, 4.3_

- [x] 5.2 状態管理とデータフローの実装
  - useStateとuseEffectを使用した状態管理
  - 取引追加後の一覧と残高の自動更新
  - エラーハンドリングとSnackbarでの通知表示
  - _Requirements: 1.2, 2.1, 3.4, 4.4_

- [x] 6. 基本テストの実装
- [x] 6.1 バックエンド単体テストの作成
  - TransactionServiceの主要メソッドのテスト
  - TransactionsControllerのエンドポイントテスト
  - バリデーションロジックのテスト
  - _Requirements: 1.3, 1.4, 2.1, 3.1_

- [x] 6.2 フロントエンドコンポーネントテストの作成
  - TransactionFormコンポーネントのテスト
  - TransactionListコンポーネントのテスト
  - BalanceDisplayコンポーネントのテスト
  - _Requirements: 1.1, 2.1, 3.1, 4.1_

- [x] 7. 統合とデプロイ準備
- [x] 7.1 フロントエンド・バックエンド統合テスト
  - 開発環境での動作確認
  - CORS設定の確認
  - エラーハンドリングの統合テスト
  - _Requirements: 1.1, 1.2, 2.1, 3.1_

- [x] 7.2 本番環境準備とドキュメント作成
  - 環境設定ファイルの作成
  - README.mdの作成（セットアップ手順）
  - 基本的な使用方法のドキュメント
  - _Requirements: 4.1, 4.2_

- [x] 8. PDF生成機能の実装
- [x] 8.1 バックエンドPDF生成基盤の構築
  - PuppeteerSharpパッケージのインストールと設定
  - IPdfServiceインターフェースとPdfServiceクラスの実装
  - HTMLテンプレート文字列の作成（IPAex明朝フォント使用）
  - _Requirements: 5.1, 5.2, 5.7_

- [x] 8.2 PDF生成エンドポイントの実装
  - TransactionsControllerにGET /api/transactions/pdfエンドポイント追加
  - 取引データの取得とHTMLテンプレートへの埋め込み
  - PuppeteerSharpを使用したPDF変換処理
  - Content-Dispositionヘッダーでファイルダウンロード設定
  - _Requirements: 5.1, 5.3, 5.4_

- [x] 8.3 PDF生成エラーハンドリングの実装
  - PDF生成失敗時のエラーレスポンス処理
  - 取引データなしの場合の適切なメッセージ表示
  - Puppeteerブラウザ起動失敗時のエラーハンドリング
  - _Requirements: 5.5, 5.6_

- [x] 8.4 フロントエンドPDFダウンロード機能の実装
  - PdfDownloadButtonコンポーネントの作成
  - APIサービスにPDFダウンロード機能の追加
  - ダウンロード中のローディング状態管理
  - エラーハンドリングとユーザー通知
  - _Requirements: 5.1, 5.4, 5.6_

- [x] 8.5 PDF機能の統合とテスト
  - メインダッシュボードにPDFダウンロードボタンの配置
  - PDF生成機能の単体テスト作成
  - フロントエンド・バックエンド統合テスト
  - 生成されるPDFの内容とフォーマット確認
  - _Requirements: 5.1, 5.2, 5.3, 5.7_

- [ ] 9. PuppeteerSharpからPlaywrightへの移行
- [x] 9.1 Playwrightパッケージの導入とPuppeteerSharpの削除
  - Microsoft.Playwright NuGetパッケージのインストール
  - PuppeteerSharpパッケージの削除
  - プロジェクトファイルの更新
  - _Requirements: 5.1_

- [x] 9.2 PdfServiceのPlaywright実装への書き換え
  - PuppeteerSharp APIからPlaywright APIへの変換
  - ブラウザ起動オプションの調整
  - PDF生成オプションの移行
  - エラーハンドリングの調整
  - _Requirements: 5.1, 5.2, 5.6_

- [x] 9.3 Dockerfileの更新
  - Chromiumインストール部分の調整
  - Playwright用の依存関係の追加
  - 環境変数の設定
  - _Requirements: 5.1_

- [x] 9.4 移行後のテストと動作確認
  - PDF生成機能の動作テスト
  - 既存のPDFテストの実行と修正
  - 統合テストでの動作確認
  - 生成されるPDFの品質確認
  - _Requirements: 5.1, 5.2, 5.3, 5.7_