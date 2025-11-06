using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Middleware
{
    /// <summary>
    /// Middleware để bỏ qua authentication trong môi trường Development
    /// Chỉ sử dụng cho testing và development, KHÔNG sử dụng trong production
    /// </summary>
    public class DevelopmentAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DevelopmentAuthMiddleware> _logger;

        public DevelopmentAuthMiddleware(RequestDelegate next, ILogger<DevelopmentAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Chỉ áp dụng trong Development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                var path = context.Request.Path.Value?.ToLower();
                
                // Bỏ qua authentication cho các endpoint test hoặc khi có query parameter ?skipAuth=true
                if (ShouldSkipAuth(path, context.Request.Query))
                {
                    _logger.LogInformation("Skipping authentication for path: {Path}", path);
                    
                    // Tạo fake claims cho test
                    var claims = CreateTestClaims(context.Request.Query);
                    var identity = new ClaimsIdentity(claims, "development");
                    context.User = new ClaimsPrincipal(identity);
                }
            }

            await _next(context);
        }

        private bool ShouldSkipAuth(string? path, IQueryCollection query)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // Bỏ qua auth cho các endpoint test
            if (path.Contains("/api/test/") || 
                path.Contains("/swagger") || 
                path.Contains("/health"))
            {
                return true;
            }

            // Bỏ qua auth khi có query parameter ?skipAuth=true
            if (query.ContainsKey("skipAuth") && 
                bool.TryParse(query["skipAuth"], out var skipAuth) && 
                skipAuth)
            {
                return true;
            }

            return false;
        }

        private List<Claim> CreateTestClaims(IQueryCollection query)
        {
            var claims = new List<Claim>();

            // Lấy role từ query parameter hoặc mặc định là admin
            var role = query.ContainsKey("role") ? query["role"].ToString() : "admin";
            var userId = query.ContainsKey("userId") ? query["userId"].ToString() : "507f1f77bcf86cd799439000";
            var email = query.ContainsKey("email") ? query["email"].ToString() : "test@example.com";

            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim(ClaimTypes.Name, "Test User"));

            return claims;
        }
    }

    /// <summary>
    /// Extension method để đăng ký middleware
    /// </summary>
    public static class DevelopmentAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseDevelopmentAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DevelopmentAuthMiddleware>();
        }
    }
}
