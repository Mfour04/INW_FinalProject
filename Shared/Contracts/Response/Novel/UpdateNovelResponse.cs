using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Response.Tag;

namespace Shared.Contracts.Response.Novel
{
    public class UpdateNovelResponse
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string NovelImage { get; set; }
        public string NovelBanner { get; set; }
        public List<TagListResponse>? Tags { get; set; } = new();
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public NovelStatus? Status { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsLock { get; set; }
        public bool? IsPaid { get; set; }
        public int? Price { get; set; }
        public long CreateAt { get; set; }
        public long UpdateAt { get; set; }
    }
}
