using Microsoft.Playwright;
using SimpleAccounting.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace SimpleAccounting.API.Services;

public class PdfService : IPdfService
{
    private readonly AccountingDbContext _context;

    public PdfService(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateTransactionsPdfAsync()
    {
        try
        {
            Console.WriteLine("PDF生成開始");
            
            // Playwright環境変数の設定とデバッグ
            var browsersPath = "/ms-playwright";
            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", browsersPath);
            
            // デバッグ情報を出力
            Console.WriteLine($"PLAYWRIGHT_BROWSERS_PATH設定: {Environment.GetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH")}");
            Console.WriteLine($"現在のユーザー: {Environment.UserName}");
            Console.WriteLine($"ホームディレクトリ: {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}");
            
            // ブラウザディレクトリの存在確認
            if (Directory.Exists(browsersPath))
            {
                Console.WriteLine($"ブラウザディレクトリが存在します: {browsersPath}");
                var directories = Directory.GetDirectories(browsersPath);
                foreach (var dir in directories)
                {
                    Console.WriteLine($"  サブディレクトリ: {dir}");
                    // Chromiumディレクトリ内をチェック
                    if (dir.Contains("chromium"))
                    {
                        var chromeFiles = Directory.GetFiles(dir, "*chrome*", SearchOption.AllDirectories);
                        foreach (var file in chromeFiles.Take(5))
                        {
                            Console.WriteLine($"    ファイル: {file}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"ブラウザディレクトリが存在しません: {browsersPath}");
                
                // 代替パスを試す
                var altPaths = new[]
                {
                    "/home/appuser/.cache/ms-playwright",
                    "/root/.cache/ms-playwright"
                };
                
                foreach (var altPath in altPaths)
                {
                    if (Directory.Exists(altPath))
                    {
                        Console.WriteLine($"代替パスが見つかりました: {altPath}");
                        Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", altPath);
                        browsersPath = altPath;
                        break;
                    }
                }
            }
            
            // 取引データを取得
            var transactions = await _context.Transactions
                .OrderBy(t => t.Date)
                .ToListAsync();
            Console.WriteLine($"取引データ取得完了: {transactions.Count}件");

            // 残高を計算
            var balance = transactions
                .Sum(t => t.Type == Models.TransactionType.Income ? t.Amount : -t.Amount);
            Console.WriteLine($"残高計算完了: {balance}");

            // HTMLテンプレートを生成
            var html = GenerateHtmlTemplate(transactions, balance);
            Console.WriteLine("HTMLテンプレート生成完了");

            // PlaywrightでPDF生成
            Console.WriteLine("Playwright初期化開始");
            
            using var playwright = await Playwright.CreateAsync();
            
            IBrowser? browser = null;
            IPage? page = null;
            
            try
            {
                Console.WriteLine("ブラウザ起動開始");
                
                browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,  // Dockerコンテナ内ではheadlessモードが必要
                    Args = new[] { 
                        "--no-sandbox", 
                        "--disable-setuid-sandbox",
                        "--disable-dev-shm-usage",
                        "--disable-extensions",
                        "--disable-gpu",
                        "--no-first-run",
                        "--disable-background-timer-throttling",
                        "--disable-backgrounding-occluded-windows",
                        "--disable-renderer-backgrounding"
                    }
                });
                Console.WriteLine("ブラウザ起動完了");

                Console.WriteLine("ページ作成開始");
                page = await browser.NewPageAsync();
                Console.WriteLine("ページ作成完了");
                
                Console.WriteLine("HTMLコンテンツ設定開始");
                await page.SetContentAsync(html);
                Console.WriteLine("HTMLコンテンツ設定完了");
                
                Console.WriteLine("PDF生成開始");
                var pdfBytes = await page.PdfAsync(new PagePdfOptions
                {
                    Format = "A4",
                    Margin = new Margin
                    {
                        Top = "20mm",
                        Right = "20mm", 
                        Bottom = "20mm",
                        Left = "20mm"
                    },
                    PrintBackground = true
                });
                Console.WriteLine("PDF生成完了");

                return pdfBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF生成エラー: {ex.Message}");
                Console.WriteLine($"スタックトレース: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部例外: {ex.InnerException.Message}");
                    Console.WriteLine($"内部例外スタックトレース: {ex.InnerException.StackTrace}");
                }
                throw new InvalidOperationException("PDF生成中にエラーが発生しました", ex);
            }
            finally
            {
                if (page != null)
                {
                    await page.CloseAsync();
                }
                if (browser != null)
                {
                    await browser.CloseAsync();
                }
            }
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException("PDF生成処理でエラーが発生しました", ex);
        }
    }

    private string GenerateHtmlTemplate(IEnumerable<Models.Transaction> transactions, decimal balance)
    {
        var html = new StringBuilder();
        
        html.AppendLine(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Noto+Serif+JP:wght@400;700&display=swap');
        
        body { 
            /* IPAex明朝を指定 */
            font-family: 'IPAexMincho', serif; 
            margin: 0;
            padding: 20px;
            line-height: 1.6;
        }
        
        h1 {
            text-align: center;
            color: #333;
            margin-bottom: 30px;
            font-size: 24px;
        }
        
        .header-info {
            text-align: center;
            margin-bottom: 30px;
            color: #666;
        }
        
        table { 
            width: 100%; 
            border-collapse: collapse; 
            margin-bottom: 30px;
            font-size: 12px;
        }
        
        th, td { 
            border: 1px solid #ddd; 
            padding: 8px; 
            text-align: left; 
        }
        
        th {
            background-color: #f5f5f5;
            font-weight: bold;
            text-align: center;
        }
        
        .amount-cell {
            text-align: right;
            font-weight: bold;
        }
        
        .income { 
            color: #2e7d32; 
        }
        
        .expense { 
            color: #d32f2f; 
        }
        
        .date-cell {
            text-align: center;
            width: 100px;
        }
        
        .type-cell {
            text-align: center;
            width: 80px;
        }
        
        .balance-summary {
            text-align: center;
            font-size: 16px;
            font-weight: bold;
            margin-top: 20px;
            padding: 15px;
            border: 2px solid #333;
            background-color: #f9f9f9;
        }
        
        .no-data {
            text-align: center;
            color: #666;
            font-style: italic;
            padding: 40px;
        }
    </style>
</head>
<body>
    <h1>仕訳帳</h1>
    <div class=""header-info"">
        <p>生成日時: " + DateTime.Now.ToString("yyyy年MM月dd日 HH:mm") + @"</p>
    </div>");

        if (transactions.Any())
        {
            html.AppendLine(@"
    <table>
        <thead>
            <tr>
                <th>日付</th>
                <th>説明</th>
                <th>金額</th>
                <th>取引タイプ</th>
            </tr>
        </thead>
        <tbody>");

            foreach (var transaction in transactions)
            {
                var typeClass = transaction.Type == Models.TransactionType.Income ? "income" : "expense";
                var typeText = transaction.Type == Models.TransactionType.Income ? "収入" : "支出";
                var amountText = transaction.Amount.ToString("N0") + "円";

                html.AppendLine($@"
            <tr>
                <td class=""date-cell"">{transaction.Date:yyyy/MM/dd}</td>
                <td>{transaction.Description}</td>
                <td class=""amount-cell {typeClass}"">{amountText}</td>
                <td class=""type-cell"">{typeText}</td>
            </tr>");
            }

            html.AppendLine(@"
        </tbody>
    </table>");
        }
        else
        {
            html.AppendLine(@"
    <div class=""no-data"">
        <p>取引データがありません</p>
    </div>");
        }

        var balanceClass = balance >= 0 ? "income" : "expense";
        var balanceText = balance.ToString("N0") + "円";

        html.AppendLine($@"
    <div class=""balance-summary"">
        <p>合計残高: <span class=""{balanceClass}"">{balanceText}</span></p>
    </div>
</body>
</html>");

        return html.ToString();
    }
}