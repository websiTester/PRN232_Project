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

		[HttpGet("{id}")]
		public async Task<IActionResult> GetSellerById(int id)
		{
			var seller = await _context.Users.Include(u => u.Feedbacks)
				.FirstOrDefaultAsync(u => u.Id == id && u.Role == "seller");

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
	}
}
