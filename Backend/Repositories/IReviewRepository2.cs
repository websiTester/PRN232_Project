using Backend.Models;

namespace Backend.Repositories
{
    public interface IReviewRepository2
    {
        Task<Product?> GetProductByIdAndSellerIdAsync(int productId, int sellerId);

        Task<OrderItem?> GetOrderItemWithOrderAsync(int orderId, int productId);

        Task<Review?> GetExistingReviewAsync(int sellerId, int productId);

        Task<Review> AddReviewAsync(Review review);

        Task<IEnumerable<Review>> GetReviewsReceivedByBuyerAsync(int buyerId);
    }
}
