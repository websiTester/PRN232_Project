using Frontend.Helpers;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Frontend.Controllers
{
    //[Authorize]
    public class ReviewController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly string _apiBaseUrl = "http://localhost:5236/api";

        public ReviewController(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = factory.CreateClient();
            _httpContextAccessor = httpContextAccessor;
        }

        //private void AddAuthTokenToRequest()
        //{
        //    var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");

        //    _httpClient.DefaultRequestHeaders.Authorization = null;
        //    if (!string.IsNullOrEmpty(token))
        //    {
        //        _httpClient.DefaultRequestHeaders.Authorization =
        //            new AuthenticationHeaderValue("Bearer", token);
        //    }
        //}

        [HttpGet]
        public IActionResult LeaveReview(int orderId, int productId)
        {
            var model = new SellerLeaveReviewViewModel
            {
                OrderId = orderId,
                ProductId = productId
            };
            return View(model);
        }


        //[Authorize(Roles = "seller, supporter")]
        [HttpPost]
        public async Task<IActionResult> LeaveReview(SellerLeaveReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                //AddAuthTokenToRequest();

                var url = $"{_apiBaseUrl}/reviews/seller-to-buyer";

                var response = await _httpClient.PostAsJsonAsync(url, model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Review saved successfully!";
                    return RedirectToAction("Index", "Home");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 401
                {
                    ModelState.AddModelError(string.Empty, "Invalid or missing token. Please log in again.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) // 403
                {
                    ModelState.AddModelError(string.Empty, "You do not have permission.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"An unexpected error occurred. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Internal error: {ex.Message}");
            }

            return View(model);
        }

        //[Authorize(Roles = "buyer")]
        [HttpGet]
        public async Task<IActionResult> ReceivedReviews()
        {
            try
            {
                //AddAuthTokenToRequest();

                var url = $"{_apiBaseUrl}/reviews/received";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var reviews = JsonSerializer.Deserialize<List<ReviewsViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return View(reviews);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ViewData["ErrorMessage"] = "You are not authorized to view this page.";
                }
                else
                {
                    ViewData["ErrorMessage"] = "Failed to load reviews.";
                }

                return View(new List<ReviewsViewModel>());
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Internal error: {ex.Message}";
                return View(new List<ReviewsViewModel>());
            }
        }
    }
}