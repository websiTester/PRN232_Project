using Backend.DTOs.Requests;
using Backend.Models;

namespace Backend.Services.Interface
{
    public interface IDisputeService
    {
        Task<Dispute> CreateDisputeAsync(DisputeCreateDto dto, int currentUserId);

        // API mới để hỗ trợ frontend
        Task<Dispute> GetDisputeForOrderAsync(int orderId, int currentUserId);

        Task<IEnumerable<DisputableOrderDto>> GetDisputableOrdersAsync(int buyerId);

    }
}
