using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class KeyTokenEntity : BaseEntity
    {
        public string token { get; set; }
        public string user_id { get; set; }
    }
}
