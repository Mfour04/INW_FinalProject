using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Shared.Contracts.Response.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.ReadingProcess
{
    public class ReadingProcessResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string NovelImage { get; set; }
        public string NovelBanner { get; set; }
        public List<TagListResponse> Tags { get; set; } = new();
        [BsonRepresentation(BsonType.String)]
        public NovelStatus Status { get; set; }
        public int TotalChapters { get; set; }
        public int TotalViews { get; set; }
        public int CommentCount { get; set; }
        public double RatingAvg { get; set; }
        public int RatingCount { get; set; }
        public string ChapterId { get; set; }
        public long CreateAt { get; set; }
        public long UpdateAt { get; set; }

    }
}
