using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configuration
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpiryMinutes { get; set; }
        public int RefreshTokenExpiryDays { get; set; }
    }

    public class RedisSettings
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
    }

    public class FileUploadSettings
    {
        public string UploadPath { get; set; } = "wwwroot/uploads";
        public long MaxFileSizeInBytes { get; set; } = 10485760; // 10MB
        public List<string> AllowedImageExtensions { get; set; } = new() { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public List<string> AllowedDocumentExtensions { get; set; } = new() { ".pdf", ".doc", ".docx" };
        public string BaseUrl { get; set; } = "https://localhost:7015";
    }
}
