using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService2 _reviewService;

        public ReviewsController(IReviewService2 reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost("seller-to-buyer")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateSellerReview([FromBody] SellerReviewCreateDto reviewDto)
        {
            try
            {
                var sellerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(sellerIdStr))
                {
                    return Unauthorized("User not found.");
                }

                var sellerId = int.Parse(sellerIdStr);

                var newReview = await _reviewService.CreateSellerReviewAsync(sellerId, reviewDto);

                var reviewResponse = new ReviewResponseDto
                {
                    Id = newReview.Id,
                    ProductId = newReview.ProductId,
                    ReviewerId = newReview.ReviewerId,
                    Rating = newReview.Rating,
                    Comment = newReview.Comment,
                    CreatedAt = newReview.CreatedAt
                };

                return Ok(reviewResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("received")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> GetReviewsReceivedByBuyer()
        {
            try
            {
                var buyerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(buyerIdStr))
                {
                    return Unauthorized("User not found.");
                }

                var buyerId = int.Parse(buyerIdStr);

                var reviews = await _reviewService.GetReviewsForBuyerAsync(buyerId);

                var reviewResponses = reviews.Select(r => new ReviewResponseDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ReviewerId = r.ReviewerId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                });

                return Ok(reviewResponses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
