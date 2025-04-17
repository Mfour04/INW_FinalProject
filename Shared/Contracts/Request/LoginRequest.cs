using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Request
{
    public class LoginRequest
    {
        [BsonElement("email")]
        public string? Email { get; set; }
        [BsonElement("passwordHash")]
        public string? PasswordHash { get; set; }
    }
}
