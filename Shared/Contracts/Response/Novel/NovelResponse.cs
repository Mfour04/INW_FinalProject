using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Contracts.Response.Tag;

namespace Shared.Contracts.Response.Novel
{
    public class NovelResponse
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string AuthorId { get; set; }
        public List<TagListResponse> Tags { get; set; } = new();
        [BsonRepresentation(BsonType.String)]
        public NovelStatus Status { get; set; }
        public bool IsPublic { get; set; }
        public bool IsPaid { get; set; }
        [BsonRepresentation(BsonType.String)]
        public PurchaseType PurchaseType { get; set; }
        public int Price { get; set; }
        public int TotalChapters { get; set; }
        public int TotalViews { get; set; }
        public int Followers { get; set; }
        public double RatingAvg { get; set; }
        public int RatingCount { get; set; }
    }
}
