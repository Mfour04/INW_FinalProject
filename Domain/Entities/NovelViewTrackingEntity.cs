using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class NovelViewTrackingEntity: BaseEntity
    {
        public string user_id { get; set; }
        public string novel_id { get; set; }
    }
}
