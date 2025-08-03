using System.Text;
using System.Text.Json;
using Application.Services.Interfaces;

namespace Application.Services.Implements
{
    public class VietQrService : IVietQrService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.vietqr.io/v2/generate";
        private const string BanksApi = "https://api.vietqr.io/v2/banks";

        public VietQrService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateWithdrawQrAsync(long accountNo, string accountName, int acqId, decimal amount, string addInfo)
        {
            var payload = new
            {
                accountNo,
                accountName,
                acqId,
                amount,
                addInfo,
                format = "text",
                template = "print"
            };

            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(ApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"VietQR API failed: {response.StatusCode} - {errorBody}");
            }

            var resultString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultString);

            if (doc.RootElement.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty("qrDataURL", out var qrDataUrlElement))
            {
                var qrDataUrl = qrDataUrlElement.GetString();
                if (!string.IsNullOrEmpty(qrDataUrl))
                    return qrDataUrl!;
            }

            throw new Exception("QR data not found in VietQR API response.");
        }

        public async Task<IEnumerable<VietQrBankModel>> GetSupportedBanksAsync()
        {
            using var response = await _httpClient.GetAsync(BanksApi);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"VietQR API failed: {response.StatusCode}");

            var resultString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultString);

            if (!doc.RootElement.TryGetProperty("data", out var dataArray) || dataArray.ValueKind != JsonValueKind.Array)
                return Enumerable.Empty<VietQrBankModel>();

            var supportedBanks = new List<VietQrBankModel>();

            foreach (var bank in dataArray.EnumerateArray())
            {
                if (bank.TryGetProperty("transferSupported", out var transferSupportedProp) &&
                    transferSupportedProp.GetInt32() == 1)
                {
                    supportedBanks.Add(new VietQrBankModel(
                        bank.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0,
                        bank.GetProperty("name").GetString() ?? string.Empty,
                        bank.GetProperty("code").GetString() ?? string.Empty,
                        bank.GetProperty("bin").GetString() ?? string.Empty,
                        bank.GetProperty("shortName").GetString() ?? string.Empty,
                        bank.GetProperty("logo").GetString() ?? string.Empty
                    ));
                }
            }

            return supportedBanks;
        }
    }
}
