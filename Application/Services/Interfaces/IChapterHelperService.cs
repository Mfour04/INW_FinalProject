namespace Application.Services.Interfaces
{
    public interface IChapterHelperService
    {
        Task<string> GetChapterAuthorIdAsync(string chapterId);
        Task ProcessViewAsync(string chapterId, string userId);
    }
}
