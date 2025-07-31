using System;
using System.Collections.Generic;

namespace Shared.Contracts.Response.OpenAI
{
    public class ModerationResult
    {
        public bool Flagged { get; set; }
        public Dictionary<string, bool> Categories { get; set; }
        public Dictionary<string, float> CategoryScores { get; set; }
    }

}
