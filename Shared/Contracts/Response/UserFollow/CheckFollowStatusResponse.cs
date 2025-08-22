namespace Shared.Contracts.Response.UserFollow
{
    public class CheckFollowStatusResponse
    {
        public bool IsFollowing { get; set; }
        public bool IsFollowedBy { get; set; }
    }
}
