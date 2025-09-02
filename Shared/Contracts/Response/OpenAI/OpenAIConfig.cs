

namespace Shared.Contracts.Response.OpenAI
{
    public class OpenAIConfig
    {
        public string ApiKey { get; set; }
        public string SummaryModel { get; set; }
        public string SummaryUrl { get; set; }
        public string EmbeddingModel { get; set; }
        public string EmbeddingUrl { get; set; }
        public string ModerationModel { get; set; }
        public string ModerationUrl { get; set; }
    }

}
