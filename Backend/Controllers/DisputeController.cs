using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // Để lấy ClaimTypes.NameIdentifier
using System.Threading.Tasks;
using System.Security.Authentication;
using Backend.Models;
using Backend.DTOs.Requests;
using Backend.Services.Interface; // Để lấy AuthenticationException

// Giả sử các interface và DTO của bạn nằm trong các namespace này
// using YourProject.Services;
// using YourProject.Dtos;
// using YourProject.Exceptions;

[Route("api/[controller]")]
[ApiController]
[Authorize] 
public class DisputesController : ControllerBase
{
    private readonly IDisputeService _disputeService;
    // (Bạn cũng có thể inject ILogger nếu cần)

    public DisputesController(IDisputeService disputeService)
    {
        _disputeService = disputeService;
    }

    /// <summary>
    /// Tạo một khiếu nại mới cho một đơn hàng.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Dispute), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateDispute([FromBody] DisputeCreateDto disputeDto)
    {
        try
        {
            // Lấy ID người dùng hiện tại từ token (claims)
            int currentUserId = GetCurrentUserId();
            Console.WriteLine(currentUserId);
            // Gọi Service để xử lý logic
            var createdDispute = await _disputeService.CreateDisputeAsync(disputeDto, currentUserId);

            // Trả về 201 Created với đối tượng vừa tạo
            return CreatedAtAction(nameof(GetDisputeById), new { id = createdDispute.Id }, createdDispute);
        }
        catch (AuthenticationException ex)
        {
            return Unauthorized(ex.Message); // 401
        }
      
        catch (Exception ex)
        {
            // Ghi log lỗi ex ở đây
            return StatusCode(500, "Đã xảy ra lỗi máy chủ nội bộ.");
        }
    }

    /// <summary>
    /// [Hỗ trợ Frontend] Lấy trạng thái khiếu nại hiện tại của một đơn hàng.
    /// Giúp frontend biết hiển thị "bước" nào khi tải lại trang.
    /// </summary>
    [HttpGet("by-order/{orderId}")]
    [ProducesResponseType(typeof(Dispute), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDisputeForOrder(int orderId)
    {
        try
        {
            int currentUserId = GetCurrentUserId();

            // Gọi Service
            var dispute = await _disputeService.GetDisputeForOrderAsync(orderId, currentUserId);

            if (dispute == null)
            {
                // Không tìm thấy không phải lỗi, chỉ là chưa có
                return NotFound("Chưa có khiếu nại nào cho đơn hàng này.");
            }

            return Ok(dispute);
        }
        catch (AuthenticationException ex)
        {
            return Unauthorized(ex.Message); // 401
        }
        catch (Exception ex)
        {
            return Forbid(ex.Message); // 403
        }
     
    }

    /// <summary>
    /// Lấy chi tiết một khiếu nại bằng ID của khiếu nại.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Dispute), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDisputeById(int id)
    {


        // Mã giả tạm thời:
        return Ok($"Đang lấy khiếu nại ID {id}. Bạn cần hoàn thiện logic này.");
    }

    // --- Phương thức Trợ giúp (Helper Method) ---



    [HttpGet("disputable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDisputableOrders()
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.Parse(userIdString);

            var orders = await _disputeService.GetDisputableOrdersAsync(currentUserId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Lỗi máy chủ nội bộ khi lấy đơn hàng.");
        }
    }







        private int GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            throw new AuthenticationException("Không thể xác định người dùng. Token không hợp lệ hoặc thiếu thông tin.");
        }

        return userId;
    }
}