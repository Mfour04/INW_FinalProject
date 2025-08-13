namespace Application.Services.Interfaces
{
    public interface ICacheService
    {
        Task<bool> Exists(string key);
        Task Set(string key, string value, TimeSpan? expiry = null);
        bool TryGetValue(string key, out string value);
    }

}
