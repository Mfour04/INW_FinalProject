using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.ReadingProcess
{
    public class UpdateReadingProcessResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
    }
}
