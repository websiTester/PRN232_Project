using Backend.DTOs.Dispute;

namespace Backend.Services
{
    public interface IDisputeService
    {
        Task<List<DisputeListItemDto>> GetDisputesByBuyerAsync(int buyerId);
        Task<List<DisputeListItemDto>> GetDisputesBySellerAsync(int sellerId);
    }
}
