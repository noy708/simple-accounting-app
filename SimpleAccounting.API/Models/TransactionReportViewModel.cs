namespace SimpleAccounting.API.Models;

public class TransactionReportViewModel
{
    public IEnumerable<Transaction> Transactions { get; set; } = new List<Transaction>();
    public decimal Balance { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}