using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response
{
    public class OpenAIConfig
    {
        public string ApiKey { get; set; }
        public string EmbeddingModel { get; set; }
        public string EmbeddingUrl { get; set; }
    }

}
