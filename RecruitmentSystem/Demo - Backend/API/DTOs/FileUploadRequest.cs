using Microsoft.AspNetCore.Http;

namespace API.DTOs
{
    public class FileUploadRequest
    {
        public IFormFile File { get; set; }
    }
}
