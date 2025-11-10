using Backend.Models; // Namespace cho DbContext và Models
using Backend.DTOs.Requests; // Namespace cho DTO mà Frontend gửi lên
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/review")] // <-- Khớp với "review" mà Frontend đang gọi
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly CloneEbayDbContext _context; // <-- Sử dụng DbContext chính xác của bạn
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(CloneEbayDbContext context, ILogger<ReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ĐÂY LÀ ENDPOINT ĐÃ GIẢI QUYẾT LỖI 404 CỦA BẠN
        // Xử lý: POST /api/review
        [HttpPost]
        [Authorize(Roles = "Buyer")] // <-- Yêu cầu token có role "Buyer" (như trong debug của bạn)
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            // 🔍 DEBUG: In ra tất cả claims
            _logger.LogInformation("===== DEBUG CLAIMS =====");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }
            _logger.LogInformation("Is Authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("========================");
            // 1. Lấy UserId (dưới dạng 'int') từ Token
            // Model 'Review' của bạn dùng 'ReviewerId' là int?, 
            // nên chúng ta cần lấy 'NameIdentifier' (thường là ID) từ token và ép kiểu sang int.
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int reviewerId))
            {
                _logger.LogWarning("Không thể tìm thấy/phân tích UserId (NameIdentifier) từ token.");
                return Unauthorized("User ID not found or invalid in token.");
            }

            try
            {
                // 2. [Logic nghiệp vụ quan trọng] Kiểm tra xem người dùng đã MUA sản phẩm này chưa?
                // Giả định rằng một đơn hàng phải ở trạng thái "Completed" (hoặc tương tự)
                // để được phép review.
                var hasPurchased = await _context.OrderTables
                    .AnyAsync(o => o.BuyerId == reviewerId &&
                                   o.Status == "Completed" && // <-- Bạn có thể cần thay đổi "Completed"
                                   o.OrderItems.Any(oi => oi.ProductId == dto.ProductId));

                if (!hasPurchased)
                {
                    _logger.LogWarning($"User {reviewerId} cố gắng review SP {dto.ProductId} mà chưa mua.");
                    // Trả về 403 Forbidden - Bạn không được phép làm điều này
                    return Forbid("You can only review items you have purchased and received.");
                }

                // 3. Kiểm tra xem người dùng đã review sản phẩm này CHƯA
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ReviewerId == reviewerId && r.ProductId == dto.ProductId);

                if (existingReview != null)
                {
                    _logger.LogWarning($"User {reviewerId} cố gắng review SP {dto.ProductId} một lần nữa.");
                    // Trả về 409 Conflict. 
                    // Frontend sẽ thấy đây là "IsSuccessStatusCode = false"
                    // và hiển thị lỗi "You may have already reviewed this item."
                    return Conflict("You have already reviewed this item.");
                }

                // 4. Tạo Review mới
                var newReview = new Review
                {
                    ProductId = dto.ProductId,
                    ReviewerId = reviewerId, // <-- Sử dụng int ID
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow // Đặt thời gian hiện tại
                };

                // 5. Lưu vào Database
                _context.Reviews.Add(newReview);
                await _context.SaveChangesAsync();

                // 6. Trả về 200 OK
                _logger.LogInformation($"User {reviewerId} đã review thành công sản phẩm {dto.ProductId}");
                return Ok(); // Frontend sẽ thấy "IsSuccessStatusCode = true"
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi nghiêm trọng khi User {reviewerId} tạo review cho SP {dto.ProductId}");
                // Trả về 500
                return StatusCode(500, "An internal server error occurred while submitting your review.");
            }
        }
    }
}