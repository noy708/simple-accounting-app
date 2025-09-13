using Microsoft.AspNetCore.Mvc;
using SimpleAccounting.API.Models.DTOs;
using SimpleAccounting.API.Services;

namespace SimpleAccounting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IPdfService _pdfService;

        public TransactionsController(ITransactionService transactionService, IPdfService pdfService)
        {
            _transactionService = transactionService;
            _pdfService = pdfService;
        }

        /// <summary>
        /// 取引一覧を取得します
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetTransactions()
        {
            try
            {
                var transactions = await _transactionService.GetAllTransactionsAsync();
                var response = transactions.Select(t => new TransactionResponseDto
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Description = t.Description,
                    Type = t.Type.ToString(),
                    Date = t.Date.ToString("yyyy-MM-dd"),
                    CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取引一覧の取得中にエラーが発生しました", error = ex.Message });
            }
        }

        /// <summary>
        /// 新しい取引を作成します
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(CreateTransactionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var transaction = await _transactionService.CreateTransactionAsync(dto);
                var response = new TransactionResponseDto
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    Type = transaction.Type.ToString(),
                    Date = transaction.Date.ToString("yyyy-MM-dd"),
                    CreatedAt = transaction.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取引の作成中にエラーが発生しました", error = ex.Message });
            }
        }

        /// <summary>
        /// 現在の残高を取得します
        /// </summary>
        [HttpGet("balance")]
        public async Task<ActionResult<decimal>> GetBalance()
        {
            try
            {
                var balance = await _transactionService.GetBalanceAsync();
                return Ok(new { balance });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "残高の取得中にエラーが発生しました", error = ex.Message });
            }
        }

        /// <summary>
        /// 仕訳帳をPDF形式でダウンロードします
        /// </summary>
        [HttpGet("pdf")]
        public async Task<IActionResult> DownloadPdf()
        {
            try
            {
                Console.WriteLine("PDF download request received");
                var pdfBytes = await _pdfService.GenerateTransactionsPdfAsync();
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    Console.WriteLine("PDF generation failed - empty result");
                    return StatusCode(500, new { message = "PDF生成に失敗しました。生成されたファイルが空です。" });
                }
                
                var fileName = $"仕訳帳_{DateTime.Now:yyyy-MM-dd}.pdf";
                Console.WriteLine($"PDF generated successfully, size: {pdfBytes.Length} bytes, filename: {fileName}");
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"InvalidOperationException in PDF generation: {ex.Message}");
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in PDF generation: {ex.Message}");
                return StatusCode(500, new { message = "PDF生成中に予期しないエラーが発生しました", error = ex.Message });
            }
        }
    }
}