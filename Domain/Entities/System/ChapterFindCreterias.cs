using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.System
{
    public class ChapterFindCreterias : PagingCreterias
    {
        public string NovelId { get; set; } = null!;
        public int? ChapterNumber { get; set; }
    }

}
