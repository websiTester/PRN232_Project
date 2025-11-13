using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Backend.ProgramConfig
{
	public static class Quan
	{
		public static IServiceCollection AddMyServices4(this IServiceCollection services)
		{
            //Configure services in program.cs here
            services.AddRateLimiter(options =>
            {
                // Chính sách 1: Giới hạn cố định (Fixed Window)
                // Áp dụng cho toàn bộ API, dựa trên địa chỉ IP
                options.AddFixedWindowLimiter(policyName: "fixed_by_ip", opt =>
                {
                    opt.PermitLimit = 100; // 100 requests
                    opt.Window = TimeSpan.FromMinutes(1); // trong 1 phút
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 5; // Xếp hàng 5 request
                });

                // Chính sách 2: Giới hạn theo User ID (Linh hoạt hơn)
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
