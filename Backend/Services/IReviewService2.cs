using Backend.DTOs.Requests;
using Backend.Models;

namespace Backend.Services
{
    public interface IReviewService2
    {
        Task<Review> CreateSellerReviewAsync(int sellerId, SellerReviewCreateDto reviewDto);
        Task<IEnumerable<Review>> GetReviewsForBuyerAsync(int buyerId);
    }
}
