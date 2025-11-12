using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests
{
    public class DisputeCreateDto
    {
        [Required(ErrorMessage = "Vui lòng cung cấp ID đơn hàng.")]
        public int OrderId { get; set; } 

        [Required(ErrorMessage = "Vui lòng chọn lý do.")]
        public string Reason { get; set; }

        public string? ReasonDetails { get; set; }

        [Required(ErrorMessage = "Vui lòng mô tả chi tiết vấn đề.")]
        [StringLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giải pháp mong muốn.")]
        public string DesiredResolution { get; set; }
    }
}
