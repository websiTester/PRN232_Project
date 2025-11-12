using Backend.DTOs.Requests;
using Backend.Models;
using Backend.Repositories.Implementation;
using Backend.Repositories.Interface;
using Backend.Services.Interface;

namespace Backend.Services.Implementation
{
    public class DisputeService : IDisputeService
    {
        private readonly IDisputeRepository _disputeRepo;
        private readonly IUnitOfWork _unitOfWork;
        public DisputeService(IDisputeRepository disputeRepository, IUnitOfWork unitOfWork)
        {
                    _disputeRepo = disputeRepository;
            _unitOfWork = unitOfWork;}

        public async Task<Dispute> CreateDisputeAsync(DisputeCreateDto dto, int currentUserId)
        {
            var order = await _disputeRepo.GetByIdAndBuyerAsync(dto.OrderId, currentUserId);
            if (order == null)
            {
                throw new Exception("Bạn không có quyền khiếu nại cho đơn hàng này hoặc đơn hàng không tồn tại.");
            }

            // 2. Logic nghiệp vụ (eBay): Kiểm tra đã có khiếu nại "Pending" chưa
            if (await _disputeRepo.HasPendingDisputeAsync(dto.OrderId))
            {
                throw new Exception("Đơn hàng này đã có một khiếu nại đang chờ xử lý.");
            }

            // 3. Tạo đối tượng
            var newDispute = new Dispute
            {
                OrderId = dto.OrderId,
                RaisedBy = currentUserId,
                Description = dto.Description,
                SubmittedDate = DateTime.UtcNow,
                Status = "1" // 
            };

            await _disputeRepo.AddAsync(newDispute);
            await _unitOfWork.SaveChangesAsync(); 

            return newDispute;

        }

        public async Task<IEnumerable<DisputableOrderDto>> GetDisputableOrdersAsync(int buyerId)
        {
            var orders = await _disputeRepo.GetDisputableOrdersAsync(buyerId);

            // Map từ Model (OrderTable) sang DTO (DisputableOrderDto)
            return orders.Select(o =>
            {
                var firstItem = o.OrderItems.FirstOrDefault();
                var product = firstItem?.Product;
                var seller = product?.Seller;
                var store = seller?.Stores.FirstOrDefault();

                return new DisputableOrderDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.Status,
                    PrimaryProductName = product?.Title ?? "Không có tên",
                    SellerName = store?.StoreName ?? seller?.Username ?? "Không rõ"
                };
            });
        }

        public async Task<Dispute> GetDisputeForOrderAsync(int orderId, int currentUserId)
        {
            var order = await _disputeRepo.GetByIdAndBuyerAsync(orderId, currentUserId);
            if (order == null)
            {
                throw new Exception("Bạn không có quyền xem thông tin của đơn hàng này.");
            }

            var dispute = await _disputeRepo.GetLatestByOrderIdAsync(orderId);

            return dispute;
        }
    }
    }

