using Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Options;
using Shared.Contracts.Response.OpenAI;
using CloudinaryDotNet.Actions;
using MongoDB.Bson.IO;

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

        public async Task<ModerationResult> CheckModerationAsync(string input)
        {
            var requestBody = new { input };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, _config.ModerationUrl)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var json = await JsonDocument.ParseAsync(stream);

            if (!json.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            {
                throw new Exception("Phản hồi moderation không hợp lệ.");
            }

            var result = results[0];

            var moderation = new ModerationResult
            {
                Flagged = result.GetProperty("flagged").GetBoolean(),
                Categories = result.GetProperty("categories").EnumerateObject()
                    .ToDictionary(p => p.Name, p => p.Value.GetBoolean()),
                CategoryScores = result.GetProperty("category_scores").EnumerateObject()
                    .ToDictionary(p => p.Name, p => p.Value.GetSingle())
            };

            // ✅ Bỏ flag nếu không vi phạm nghiêm trọng
            var serious = new[] { "sexual", "hate/threatening", "violence/graphic", "self-harm/instructions" };

            var isSerious = moderation.Categories
                .Where(c => c.Value)
                .Any(c => serious.Contains(c.Key) && moderation.CategoryScores[c.Key] > 0.85f);

            if (!isSerious)
            {
                moderation.Flagged = false;
            }

            return moderation;
        }


        public async Task<List<List<float>>> GetEmbeddingAsync(List<string> inputs)
        {
            var body = new
            {
                model = _config.EmbeddingModel,
                input = inputs
            };

            Console.WriteLine("INPUT TO OPENAI:");
            Console.WriteLine(JsonSerializer.Serialize(body));

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, _config.EmbeddingUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI error: {response.StatusCode} - {error}");
            }

            var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var embeddings = doc.RootElement
                .GetProperty("data")
                .EnumerateArray()
                .Select(item => item.GetProperty("embedding")
                    .EnumerateArray()
                    .Select(x => x.GetSingle())
                    .ToList()
                ).ToList();

            return embeddings;
        }

    }
}
