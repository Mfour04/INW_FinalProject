using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BaseEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public long created_at { get; set; }
        public long updated_at { get; set; }
    }
}
