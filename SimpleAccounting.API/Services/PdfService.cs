using Microsoft.Playwright;
using SimpleAccounting.API.Data;
using Microsoft.EntityFrameworkCore;
using SimpleAccounting.API.Models;

namespace SimpleAccounting.API.Services;

public class PdfService : IPdfService
{
    private readonly AccountingDbContext _context;
    private readonly ITemplateRenderingService _templateRenderingService;

    public PdfService(AccountingDbContext context, ITemplateRenderingService templateRenderingService)
    {
        _context = context;
        _templateRenderingService = templateRenderingService;
    }

    private async Task EnsurePlaywrightBrowsersInstalledAsync()
    {
        try
        {
            // Playwrightブラウザが既にインストールされているかチェック
            using var playwright = await Playwright.CreateAsync();
            
            // テスト用にブラウザ起動を試行
            try
            {
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });
                await browser.CloseAsync();
                Console.WriteLine("Playwrightブラウザは既にインストールされています");
                return;
            }
            catch (Exception ex) when (ex.Message.Contains("Executable doesn't exist"))
            {
                Console.WriteLine("Playwrightブラウザがインストールされていません。自動インストールを実行します...");
                
                // Microsoft.Playwright.Program.Main を使用してブラウザをインストール
                var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
                
                if (exitCode == 0)
                {
                    Console.WriteLine("Playwrightブラウザのインストールが完了しました");
                }
                else
                {
                    Console.WriteLine($"Playwrightブラウザのインストールに失敗しました。終了コード: {exitCode}");
                    throw new InvalidOperationException("Playwrightブラウザのインストールに失敗しました");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Playwrightブラウザの確認中にエラーが発生しました: {ex.Message}");
            throw;
        }
    }

    public async Task<byte[]> GenerateTransactionsPdfAsync()
    {
        try
        {
            Console.WriteLine("PDF生成開始");
            
            // Playwrightブラウザの自動インストールを確認
            await EnsurePlaywrightBrowsersInstalledAsync();
            
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
            Console.WriteLine("ViewModel作成開始");
            var viewModel = new TransactionReportViewModel
            {
                Transactions = transactions,
                Balance = balance,
                GeneratedAt = DateTime.Now
            };
            Console.WriteLine("ViewModel作成完了");
            
            Console.WriteLine("テンプレートレンダリング開始");
            string html = await _templateRenderingService.RenderTemplateAsync("TransactionReport", viewModel);
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
}