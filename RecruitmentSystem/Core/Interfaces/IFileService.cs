using Core.DTOs.Common;
using System.IO;

namespace Core.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadResponse> UploadFileAsync(Stream fileStream, string fileName, long fileSize, string contentType, string fileType);
        Task<bool> DeleteFileAsync(string fileName);
        bool ValidateFile(string fileName, long fileSize, string fileType, out List<string> errors);
        string GetFileUrl(string fileName);
    }
}
