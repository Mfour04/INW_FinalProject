using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;


namespace Infrastructure.Repositories.Implements
{
    public class CloudDinaryService : ICloudDinaryService
    {
        private readonly Cloudinary _cloudDinary;
        public CloudDinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["CloudinarySetttings:CloudName"];
            var apiKey = configuration["CloudinarySetttings:ApiKey"];
            var apiSecret = configuration["CloudinarySetttings:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudDinary = new Cloudinary(account);
        }

        public async Task<string> UploadImagesAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var uploadResult = await _cloudDinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.AbsoluteUri;
        }
    }
}
