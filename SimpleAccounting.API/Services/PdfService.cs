using PuppeteerSharp;
using PuppeteerSharp.Media;
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
            // 取引データを取得
            var transactions = await _context.Transactions
                .OrderBy(t => t.Date)
                .ToListAsync();

            // 残高を計算
            var balance = transactions
                .Sum(t => t.Type == Models.TransactionType.Income ? t.Amount : -t.Amount);

            // HTMLテンプレートを生成
            var html = GenerateHtmlTemplate(transactions, balance);

            // PuppeteerSharpでPDF生成
            try
            {
                await new BrowserFetcher().DownloadAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Chromiumブラウザのダウンロードに失敗しました", ex);
            }
            
            IBrowser? browser = null;
            IPage? page = null;
            
            try
            {
                browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,  // Dockerコンテナ内ではheadlessモードが必要
                    Args = new[] { 
                        "--no-sandbox", 
                        "--disable-setuid-sandbox", 
                        "--disable-dev-shm-usage",
                        "--disable-gpu",
                        "--disable-web-security",
                        "--disable-features=VizDisplayCompositor"
                    }
                });

                page = await browser.NewPageAsync();
                await page.SetContentAsync(html);
                
                var pdfBytes = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions
                    {
                        Top = "20mm",
                        Right = "20mm", 
                        Bottom = "20mm",
                        Left = "20mm"
                    },
                    PrintBackground = true
                });

                return pdfBytes ?? Array.Empty<byte>();
            }
            catch (Exception ex)
            {
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
            font-family: 'Noto Serif JP', serif; 
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