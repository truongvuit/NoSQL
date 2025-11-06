# Recruitment System API

API tuyển dụng xây dựng trên ASP.NET Core 8, MongoDB và Redis. Hỗ trợ các vai trò (Public/Candidate/Recruiter/Admin), quản lý công ty và tin tuyển dụng, nộp hồ sơ, upload tệp, và dashboard quản trị.

## Chạy nhanh

1) Yêu cầu
- .NET 8 SDK
- MongoDB, Redis (local hoặc Docker)

2) Cấu hình
- Cập nhật appsettings.json (DatabaseSettings, RedisSettings, JwtSettings, FileUploadSettings) hoặc dùng biến môi trường.

3) Build & Run

```powershell
dotnet build
# chạy bằng VS/VSCode hoặc
# dotnet run --project API/API.csproj
```

- Swagger: https://localhost:7015/swagger
- Static uploads: https://localhost:7015/uploads/
