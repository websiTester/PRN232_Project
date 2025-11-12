using Frontend.Helpers;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Frontend.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApiClientHelper _apiClient;

        public ReviewController(ApiClientHelper apiClient)
        {
            _apiClient = apiClient;
        }

        [Authorize(Roles = "Seller")]
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

        [Authorize(Roles = "Seller")]
        [HttpPost]
        public async Task<IActionResult> LeaveReview(SellerLeaveReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiClient.PostAsync("api/reviews/seller-to-buyer", model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Review saved successfully!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Internal error: {ex.Message}");
            }

            return View(model);
        }

        [Authorize(Roles = "Buyer")]
        [HttpGet]
        public async Task<IActionResult> ReceivedReviews()
        {
            try
            {
                var response = await _apiClient.GetAsync("api/reviews/received");

                if (response.IsSuccessStatusCode)
                {
                    var reviews = await response.Content.ReadFromJsonAsync<List<ReviewsViewModel>>();
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