using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _http;

        public AuthController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("API");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> LoginApi([FromBody] UserLoginViewModel model)
        {
            var response = await _http.PostAsJsonAsync("auth/login", model);
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return Json(result);
        }

    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
