namespace SimpleAccounting.API.Services;

public interface IPdfService
{
    Task<byte[]> GenerateTransactionsPdfAsync();
}