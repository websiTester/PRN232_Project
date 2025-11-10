using Backend.Models;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Frontend.Controllers
{
	public class FeedbackController : Controller
	{
		private readonly HttpClient _httpClient;

		public FeedbackController(IHttpClientFactory factory)
		{
			_httpClient = factory.CreateClient();
		}
		public async Task<IActionResult> OverviewFeedback()
		{
			

			return View();
		}

		public async Task<IActionResult> FormFeedback(int? id = 3)
		{
			var urlOrder = $"http://localhost:5236/api/Feedback/order/{id}";
			var responseOrder = await _httpClient.GetAsync(urlOrder);
			var json = await responseOrder.Content.ReadAsStringAsync();
			var order = JsonSerializer.Deserialize<OrderTable>(json, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});
			FormFeedbackViewModel model = new FormFeedbackViewModel();
			model.OrderTable = order;
			return View(model);
		}

		[HttpPost]
		public IActionResult FormFeedback(FormFeedbackViewModel model)
		{


			return View();
		}
	}
}
