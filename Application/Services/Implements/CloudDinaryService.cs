using Application.Services.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Entities.System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Services.Implements
{
    public class CloudDinaryService : ICloudDinaryService
    {
        private readonly Cloudinary _cloudDinary;
        public CloudDinaryService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;
            Console.WriteLine($"CloudName: {settings.CloudName}");
            Console.WriteLine($"ApiKey: {settings.ApiKey}");
            Console.WriteLine($"ApiSecret: {settings.ApiSecret}");
            
            if (string.IsNullOrWhiteSpace(settings.CloudName))
                throw new Exception("❌ CloudName is null or empty – check your appsettings.json");

            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudDinary = new Cloudinary(account);

        }

        public async Task<string?> UploadImagesAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                throw new ArgumentException("File type is not allowed");
            }

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var uploadResult = await _cloudDinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK || uploadResult.SecureUrl == null)
            {
                throw new Exception("Image upload failed: " + uploadResult.Error?.Message);
            }

            return uploadResult.SecureUrl.AbsoluteUri;
        }
    }
}
