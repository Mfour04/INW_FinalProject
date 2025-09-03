using Microsoft.AspNetCore.Http;

namespace Application.Services.Interfaces
{
    public interface ICloudDinaryService
    {
        Task<string?> UploadImagesAsync(IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<List<string>> UploadMultipleImagesAsync(List<IFormFile>? files, string folder);
        Task<bool> DeleteMultipleImagesAsync(List<string>? imageUrls);
    }
}
