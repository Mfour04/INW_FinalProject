using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Shared.Contracts.Response.Novel
{
    public class CreateNovelResponse
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string AuthorId { get; set; }
        public IFormFile? NovelImage { get; set; }
        public List<string> Tags { get; set; } = new();
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public NovelStatus Status { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsPaid { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public PurchaseType PurchaseType { get; set; }
        public int? Price { get; set; }
    }
}
