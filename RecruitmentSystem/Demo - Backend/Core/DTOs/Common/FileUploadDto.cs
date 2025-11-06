using System;
using System.Collections.Generic;

namespace Core.DTOs.Common
{
    public class FileUploadResponse
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileUrl { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class FileValidationError
    {
        public string Field { get; set; }
        public string Message { get; set; }
    }
}
