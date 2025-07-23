using Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Shared.Contracts.Response;
using Microsoft.Extensions.Options;

namespace Application.Services.Implements
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIConfig _config;

        public OpenAIService(HttpClient httpClient, IOptions<OpenAIConfig> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
        }
        public async Task<List<float>> GetEmbeddingAsync(List<string> tags)
        {
            var inputText = string.Join(", ", tags);

            var body = new
            {
                model = _config.EmbeddingModel,
                input = inputText
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, _config.EmbeddingUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var embedding = doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToList();

            return embedding;
        }
    }
}
