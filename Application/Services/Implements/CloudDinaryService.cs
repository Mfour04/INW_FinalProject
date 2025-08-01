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

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return false;

            try
            {
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath; // e.g., /image/upload/v123456789/Novel/abc.jpg

                // Cắt mọi thứ sau /upload/ để lấy path chuẩn
                var uploadIndex = path.IndexOf("/upload/");
                if (uploadIndex == -1)
                    return false;

                var afterUpload = path.Substring(uploadIndex + "/upload/".Length); // v123456789/Novel/abc.jpg

                var segments = afterUpload.Split('/').ToList();

                // Nếu sau upload/ là version (v123456...), thì bỏ nó
                if (segments[0].StartsWith("v") && long.TryParse(segments[0].Substring(1), out _))
                {
                    segments.RemoveAt(0); // bỏ v1753862750
                }

                var publicIdWithExt = string.Join("/", segments); // Novel/abc.jpg
                var publicId = Path.ChangeExtension(publicIdWithExt, null); // Novel/abc

                Console.WriteLine($"🧪 Extracted publicId: {publicId}");

                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudDinary.DestroyAsync(deletionParams);

                Console.WriteLine($"🗑️ Deleting publicId: {publicId} → Result: {result.Result}");
                if (result.Error != null)
                    Console.WriteLine($"❌ Error: {result.Error.Message}");

                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exception deleting image: " + ex.Message);
                return false;
            }
        }

        public async Task<string?> UploadImagesAsync(IFormFile? file, string folder)
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
                Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                Folder = folder
            };

            var uploadResult = await _cloudDinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK || uploadResult.SecureUrl == null)
            {
                throw new Exception("Image upload failed: " + uploadResult.Error?.Message);
            }

            return uploadResult.SecureUrl.AbsoluteUri;
        }

        public async Task<List<string>> UploadMultipleImagesAsync(List<IFormFile>? files, string folder)
        {
            if (files == null || files.Count == 0)
                return new List<string>();

            var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg", "image/png", "image/webp", "image/gif"
            };

            var uploadTasks = files
                .Where(file => file != null && file.Length > 0)
                .Select(async file =>
                {
                    if (!allowedTypes.Contains(file.ContentType))
                        throw new ArgumentException($"File '{file.FileName}' type is not allowed");

                    using var stream = file.OpenReadStream();

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                        Folder = folder
                    };

                    var uploadResult = await _cloudDinary.UploadAsync(uploadParams);

                    if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK || uploadResult.SecureUrl == null)
                        throw new Exception($"Image upload failed: {uploadResult.Error?.Message}");

                    return uploadResult.SecureUrl.AbsoluteUri;
                });

            return (await Task.WhenAll(uploadTasks)).ToList();
        }
    }
}
