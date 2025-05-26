using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class NovelEntity: BaseEntity
    {

        public string title { get; set; }

        public string description { get; set; }

        public string author_id { get; set; }

        public List<string> genres { get; set; } = new();

        public List<string> tags { get; set; } = new();

        public string status { get; set; } // "ongoing", "completed", "hiatus"
        public bool is_premium { get; set; }

        public double price { get; set; }

        public int total_views { get; set; }

        public int total_chapters { get; set; }

        public float rating_avg { get; set; }

        public int rating_count { get; set; }

        public int followers { get; set; }
    }
}
