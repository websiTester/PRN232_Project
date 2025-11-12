using Backend.DTOs.Requests;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class ReviewService2 : IReviewService2
    {
        private readonly IReviewRepository2 _repo;
        
        public ReviewService2(IReviewRepository2 repo)
        {
            _repo = repo;
        }


        public async Task<Review> CreateSellerReviewAsync(int sellerId, SellerReviewCreateDto reviewDto)
        {
            var product = await _repo.GetProductByIdAndSellerIdAsync(reviewDto.ProductId, sellerId);

            if (product == null)
            {
                throw new Exception("Product not found or does not belong to this seller.");
            }

            var orderItem = await _repo.GetOrderItemWithOrderAsync(reviewDto.OrderId, reviewDto.ProductId);

            if (orderItem == null)
            {
                throw new Exception("Order not found or does not contain this product.");
            }

            if (orderItem.Order.BuyerId == sellerId)
            {
                throw new Exception("Seller cannot review their own order.");
            }

            var existingReviewSimple = await _repo.GetExistingReviewAsync(sellerId, reviewDto.ProductId);

            if (existingReviewSimple != null)
            {
                throw new Exception("You have already reviewed the buyer for this product.");
            }

            var newReview = new Review
            {
                ProductId = reviewDto.ProductId,
                ReviewerId = sellerId,
                Rating = null,
                Comment = reviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            return await _repo.AddReviewAsync(newReview);
        }

        public async Task<IEnumerable<Review>> GetReviewsForBuyerAsync(int buyerId)
        {
            var reviews = await _repo.GetReviewsReceivedByBuyerAsync(buyerId);

            if (reviews == null || !reviews.Any())
            {
                return new List<Review>();
            }

            return reviews;
        }
    }
}
