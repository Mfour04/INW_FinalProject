namespace Application.Services.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Role { get; }
        bool IsAdmin();
    }
}
