namespace Application.Services.Interfaces
{
    public interface IBadgeProgressService
    {
        Task InitializeUserBadgeProgress(string userId);
    }
}