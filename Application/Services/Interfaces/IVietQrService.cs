namespace Application.Services.Interfaces
{
    public interface IVietQrService
    {
        Task<string> GenerateWithdrawQrAsync(long accountNo, string accountName, int acqId, decimal amount, string addInfo);
        Task<IEnumerable<VietQrBankModel>> GetSupportedBanksAsync();
    }

    public record VietQrBankModel(
        int Id,
        string Name,
        string Code,
        string Bin,
        string ShortName,
        string Logo
    );
}