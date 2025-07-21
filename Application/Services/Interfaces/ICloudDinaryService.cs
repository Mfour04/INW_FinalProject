using Microsoft.AspNetCore.Http;

namespace Application.Services.Interfaces
{
    public interface ICloudDinaryService
    {
        Task<string?> UploadImagesAsync(IFormFile file);
    }
}
