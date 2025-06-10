using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Ownership
{
    public class CreatePurchaserResponse
    {
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public List<string> ChapterId { get; set; } = new();
        public bool IsFull { get; set; }
    }
}
