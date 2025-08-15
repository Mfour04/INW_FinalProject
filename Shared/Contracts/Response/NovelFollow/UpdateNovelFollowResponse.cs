using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.NovelFollow
{
    public class UpdateNovelFollowResponse
    {
        public string NovelFollowId { get; set; }
        public string NovelId { get; set; }
        public string UserId { get; set; }
        public bool IsNotification { get; set; }
        public NovelFollowReadingStatus ReadingStatus { get; set; }
        public long FollowedAt { get; set; }
    }
}
