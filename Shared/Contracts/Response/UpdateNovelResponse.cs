using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Shared.Contracts.Response
{
    public class UpdateNovelResponse
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public List<string>? Tags { get; set; } = new();
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public NovelStatus? Status { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsPaid { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public PurchaseType? PurchaseType { get; set; }
        public int? Price { get; set; }
    }
}
