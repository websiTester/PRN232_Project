using Backend.DTOs.Requests;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
	[ApiController]
	[Route("api/[Controller]")]
	public class FeedbackController : Controller
	{
		private readonly CloneEbayDbContext _context;

		public FeedbackController(CloneEbayDbContext context)
		{
			_context = context;
		}

		[HttpGet("seller/{sellerId}")]
		public async Task<IActionResult> GetSellerById(int sellerId)
		{
			var seller = await _context.Users.Include(u => u.Feedbacks)
				.FirstOrDefaultAsync(u => u.Id == sellerId && u.Role == "seller");

			var negative = seller!.Feedbacks.Where(f => f.PositiveRate == -1).Count();
			var neutral = seller!.Feedbacks.Where(f => f.PositiveRate == 0).Count();
			var positive = seller!.Feedbacks.Where(f => f.PositiveRate == 1).Count();
			var sellerResponse = new
			{
				sellerId = seller.Id,
				sellerName = seller.Username,
				numberOfPositive = positive,
				numberOfNeutral = neutral,
				numberOfNegative = negative
			};
				
			return Ok();
		}


		[HttpGet("order/{orderId}")]
		public async Task<IActionResult> GetOrderById(int orderId)
		{
			var order = await _context.OrderTables.Include(ot => ot.OrderItems)
				.ThenInclude(oi => oi.Product)
				.ThenInclude(p => p.Seller)
				.FirstOrDefaultAsync(o => o.Id == orderId);

			return Ok(order);
		}


		[HttpPost("order/addfeedback/{orderId}")]
		public async Task<IActionResult> AddFeedbackForOrder(int orderId, [FromBody] FeedbackDto feedbackDto)
		{
			

			return Ok();
		}






	}
}
