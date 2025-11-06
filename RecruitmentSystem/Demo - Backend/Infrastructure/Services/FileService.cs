using Core.DTOs.Common;
using Core.Interfaces;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.IO;

namespace Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly FileUploadSettings _settings;

        public FileService(IOptions<FileUploadSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<FileUploadResponse> UploadFileAsync(Stream fileStream, string fileName, long fileSize, string contentType, string fileType)
        {
            if (!ValidateFile(fileName, fileSize, fileType, out var errors))
            {
                throw new InvalidOperationException(string.Join(", ", errors));
            }

            // Create upload directory if not exists
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.UploadPath);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Create subfolder based on file type
            var subFolder = fileType.ToLower();
            var targetPath = Path.Combine(uploadPath, subFolder);
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(targetPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            // Generate URL
            var fileUrl = GetFileUrl($"{subFolder}/{uniqueFileName}");

            return new FileUploadResponse
            {
                FileName = uniqueFileName,
                FilePath = $"uploads/{subFolder}/{uniqueFileName}",
                FileUrl = fileUrl,
                FileSize = fileSize,
                ContentType = contentType,
                UploadedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), _settings.UploadPath, fileName);
                
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateFile(string fileName, long fileSize, string fileType, out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrEmpty(fileName) || fileSize == 0)
            {
                errors.Add("File không được để trống");
                return false;
            }

            // Check file size
            if (fileSize > _settings.MaxFileSizeInBytes)
            {
                errors.Add($"Kích thước file vượt quá giới hạn {_settings.MaxFileSizeInBytes / 1024 / 1024}MB");
            }

            // Check file extension
            var fileExtension = Path.GetExtension(fileName).ToLower();
            var allowedExtensions = fileType.ToLower() switch
            {
                "image" => _settings.AllowedImageExtensions,
                "document" or "cv" => _settings.AllowedDocumentExtensions,
                _ => new List<string>()
            };

            if (!allowedExtensions.Contains(fileExtension))
            {
                errors.Add($"Định dạng file không được hỗ trợ. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}");
            }

            return errors.Count == 0;
        }

        public string GetFileUrl(string fileName)
        {
            return $"{_settings.BaseUrl}/uploads/{fileName}";
        }
    }
}
