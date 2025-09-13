using System.ComponentModel.DataAnnotations;

namespace SimpleAccounting.API.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public TransactionType Type { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum TransactionType
    {
        Income,
        Expense
    }
}