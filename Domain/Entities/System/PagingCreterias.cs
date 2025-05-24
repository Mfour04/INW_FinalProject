using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.System
{
    public class PagingCreterias
    {
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
    }
}
