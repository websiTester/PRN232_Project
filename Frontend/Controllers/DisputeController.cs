using Backend.DTOs.Dispute;
using Backend.DTOs.Responses;
using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class DisputeController : Controller
    {
        private readonly ApiClientHelper _apiClient;
        private readonly ILogger<DisputeController> _logger;
        public DisputeController(ApiClientHelper apiClient, ILogger<DisputeController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }
        [Route("dispute/buyer/{buyerId}")]
        [HttpGet]
        public async Task<IActionResult> BuyerDispute(
    int buyerId,
    int? statusFilter,          // 1,2,3,4 hoặc null = tất cả
    string? timeFilter          // "30", "60", "all"
)
        {
            try
            {
                if (buyerId <= 0)
                {
                    _logger.LogWarning("buyerId không hợp lệ: {buyerId}", buyerId);
                    return View(new List<DisputeListItemDto>());
                }

                // Gọi API: GET /api/disputes/buyer/{buyerId}
                var response = await _apiClient.GetAsync($"disputes/buyer/{buyerId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Lỗi gọi API: {status} {reason}",
                        response.StatusCode, response.ReasonPhrase);
                    return View(new List<DisputeListItemDto>());
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var disputes = JsonSerializer.Deserialize<List<DisputeListItemDto>>(content, options)
                              ?? new List<DisputeListItemDto>();

                // ============ APPLY FILTER ============

                var query = disputes.AsQueryable();

                // 1. Filter theo status cụ thể (1,2,3,4)
                if (statusFilter.HasValue)
                {
                    if (statusFilter.Value == 1) // Chưa phản hồi
                    {
                        query = query.Where(d => d.Status == "1" || d.Status == "3");
                    }
                    else if (statusFilter.Value == 2) // Đã phản hồi
                    {
                        query = query.Where(d => d.Status == "2" || d.Status == "4");
                    }

                }


                // 3. Filter khoảng thời gian
                if (!string.IsNullOrWhiteSpace(timeFilter) && timeFilter != "all")
                {
                    DateTime fromDate;

                    if (timeFilter == "30")
                        fromDate = DateTime.UtcNow.AddDays(-30);
                    else if (timeFilter == "60")
                        fromDate = DateTime.UtcNow.AddDays(-60);
                    else
                        fromDate = DateTime.MinValue;

                    query = query.Where(d => d.SubmittedDate.HasValue &&
                                             d.SubmittedDate.Value >= fromDate);
                }

                // 4. Sắp xếp: status 1 & 3 (chưa phản hồi) lên trước
                var result = query
                    .ToList();

                // Lưu giá trị filter để view đánh dấu selected
                ViewBag.buyerId = buyerId;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.TimeFilter = timeFilter;

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi lấy danh sách khiếu nại cho buyerId: {buyerId}", buyerId);
                return View("Error");
            }
        }

        [Route("dispute/seller/{sellerId}")]
        [HttpGet]
        public async Task<IActionResult> SellerDispute(int sellerId, int? statusFilter,          // 1,2,3,4 hoặc null = tất cả
    string? timeFilter)          // "30", "60", "all"
        {
            try
            {
                if (sellerId <= 0)
                {
                    _logger.LogWarning("buyerId không hợp lệ: {sellerId}", sellerId);
                    return View(new List<DisputeListItemDto>());
                }

                // Gọi API: GET /api/disputes/buyer/{buyerId}
                var response = await _apiClient.GetAsync($"disputes/seller/{sellerId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Lỗi gọi API: {status} {reason}",
                        response.StatusCode, response.ReasonPhrase);
                    return View(new List<DisputeListItemDto>());
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var disputes = JsonSerializer.Deserialize<List<DisputeListItemDto>>(content, options)
                              ?? new List<DisputeListItemDto>();

                // ============ APPLY FILTER ============

                var query = disputes.AsQueryable();

                // 1. Filter theo status cụ thể (1,2,3,4)
                if (statusFilter.HasValue)
                {
                    if (statusFilter.Value == 1) // Chưa phản hồi
                    {
                        query = query.Where(d => d.Status == "1");
                    }
                    else if (statusFilter.Value == 2) // Đã phản hồi
                    {
                        query = query.Where(d => d.Status == "2");
                    }

                }


                // 3. Filter khoảng thời gian
                if (!string.IsNullOrWhiteSpace(timeFilter) && timeFilter != "all")
                {
                    DateTime fromDate;

                    if (timeFilter == "30")
                        fromDate = DateTime.UtcNow.AddDays(-30);
                    else if (timeFilter == "60")
                        fromDate = DateTime.UtcNow.AddDays(-60);
                    else
                        fromDate = DateTime.MinValue;

                    query = query.Where(d => d.SubmittedDate.HasValue &&
                                             d.SubmittedDate.Value >= fromDate);
                }

                // 4. Sắp xếp: status 1 & 3 (chưa phản hồi) lên trước
                var result = query
                    .ToList();

                // Lưu giá trị filter để view đánh dấu selected
                ViewBag.sellerId = sellerId;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.TimeFilter = timeFilter;

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi lấy danh sách khiếu nại cho buyerId: {sellerId}", sellerId);
                return View("Error");
            }

        }
        [Route("dispute/supporter")]
        [HttpGet]
        public async Task<IActionResult> SupporterDispute( int? statusFilter, string? timeFilter)          // "30", "60", "all"
        {
            
                // Gọi API: GET /api/disputes/buyer/{buyerId}
                var response = await _apiClient.GetAsync($"disputes/supporter");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Lỗi gọi API: {status} {reason}",
                        response.StatusCode, response.ReasonPhrase);
                    return View(new List<DisputeListItemDto>());
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var disputes = JsonSerializer.Deserialize<List<DisputeListItemDto>>(content, options)
                              ?? new List<DisputeListItemDto>();

                // ============ APPLY FILTER ============

                var query = disputes.AsQueryable();

                // 1. Filter theo status cụ thể (1,2,3,4)
                if (statusFilter.HasValue)
                {
                    if (statusFilter.Value == 1) // Chưa phản hồi
                    {
                        query = query.Where(d => d.Status == "3");
                    }
                    else if (statusFilter.Value == 2) // Đã phản hồi
                    {
                        query = query.Where(d => d.Status == "4");
                    }

                }


                // 3. Filter khoảng thời gian
                if (!string.IsNullOrWhiteSpace(timeFilter) && timeFilter != "all")
                {
                    DateTime fromDate;

                    if (timeFilter == "30")
                        fromDate = DateTime.UtcNow.AddDays(-30);
                    else if (timeFilter == "60")
                        fromDate = DateTime.UtcNow.AddDays(-60);
                    else
                        fromDate = DateTime.MinValue;

                    query = query.Where(d => d.SubmittedDate.HasValue &&
                                             d.SubmittedDate.Value >= fromDate);
                }

                // 4. Sắp xếp: status 1 & 3 (chưa phản hồi) lên trước
                var result = query
                    .ToList();

                ViewBag.StatusFilter = statusFilter;
                ViewBag.TimeFilter = timeFilter;

                return View(result);
            

        }

        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public async Task<IActionResult> Respond([FromForm] RespondDisputeDto form)
        //{
        //    try
        //    {
        //        if (form.Id <= 0 || string.IsNullOrWhiteSpace(form.Resolution))
        //        {
        //            TempData["Error"] = "Thiếu dữ liệu phản hồi.";
        //            return RedirectToAction("Seller", "Dispute");
        //        }

        //        // Map Resolution -> status (không cho client tự gửi)
        //        form.status = form.Resolution == "full-refund" ? "4" : "2";
        //        form.SolvedDate = null; // Backend sẽ tự set nếu status == "4"

        //        var res = await _apiClient.PostAsJsonAsync("disputes/respond", form);
        //        if (!res.IsSuccessStatusCode)
        //        {
        //            var err = await res.Content.ReadAsStringAsync();
        //            TempData["Error"] = $"Gửi phản hồi thất bại: {err}";
        //            return RedirectToAction("Seller", "Dispute");
        //        }

        //        TempData["Success"] = "Đã gửi phản hồi thành công.";
        //        return RedirectToAction("Seller", "Dispute");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi gửi phản hồi");
        //        TempData["Error"] = "Đã xảy ra lỗi hệ thống.";
        //        return RedirectToAction("Seller", "Dispute");
        //    }
        //}
    }
}
