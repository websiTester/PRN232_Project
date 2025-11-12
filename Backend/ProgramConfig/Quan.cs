using Backend.Repositories.Implementation;
using Backend.Repositories.Interface;
using Backend.Services.Implementation;
using Backend.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

namespace Backend.ProgramConfig
{
    public static class Quan
    {
        public static IServiceCollection AddMyServices4(this IServiceCollection services)
        {
  



            services.AddScoped<IDisputeRepository, DisputeRepository>();
            services.AddScoped<IDisputeService, DisputeService>();
            //Configure services in program.cs here
            services.AddRateLimiter(options =>
            {
                // Chính sách 1: Giới hạn cố định (Fixed Window)
                // Áp dụng cho toàn bộ API, dựa trên địa chỉ IP
                options.AddSlidingWindowLimiter(policyName: "fixed_by_ip", opt =>
                {
                    opt.PermitLimit = 100; // 100 requests
                    opt.Window = TimeSpan.FromMinutes(1); // trong 1 phút
                    opt.SegmentsPerWindow = 6;

                    opt.QueueLimit = 0;
                });

                // Chính sách 2: Giới hạn theo User ID 
                // Cần xác thực (authentication) trước
                options.AddPolicy("fixed_by_user", httpContext =>
                {
                    // Lấy ID của user đã đăng nhập
                    var userId = httpContext.User.Identity?.Name ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey: userId, factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 50, // 50 requests
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // Trả về lỗi 429 khi bị giới hạn
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });
            return services;
        }
    }
}
