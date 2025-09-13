using System.ComponentModel.DataAnnotations;

namespace SimpleAccounting.API.Models.DTOs
{
    public class CreateTransactionDto
    {
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public TransactionType Type { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
    }
}