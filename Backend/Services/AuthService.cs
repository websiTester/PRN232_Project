using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Models;
using Backend.Repositories;
using Backend.Utils;
using Microsoft.AspNetCore.Identity;

namespace Backend.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepo;
        private readonly JwtUtils _jwt;
        private readonly PasswordHasher<User> _hasher;

        public AuthService(UserRepository userRepo, JwtUtils jwt)
        {
            _userRepo = userRepo;
            _jwt = jwt;
            _hasher = new PasswordHasher<User>();
        }

        public async Task<(bool Success, string Message)> RegisterAsync(UserRegisterDto dto)
        {
            var existing = await _userRepo.GetByUsernameAsync(dto.Username);
            if (existing != null)
                return (false, "Username already exists");
            var validRoles = new[] { "Buyer", "Seller", "Admin" };
            if (!validRoles.Contains(dto.Role, StringComparer.OrdinalIgnoreCase))
                return (false, "Invalid role. Must be Buyer, Seller, or Admin.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Role = dto.Role,
            };

            user.Password = _hasher.HashPassword(user, dto.Password);

            await _userRepo.AddUserAsync(user);
            return (true, "User registered successfully");
        }

        public async Task<(bool Success, string? Token, UserResponseDto? User, string Message)> LoginAsync(UserLoginDto dto)
        {
            var user = await _userRepo.GetByUsernameOrEmailAsync(dto.UsernameOrEmail);
            if (user == null)
                return (false, null, null, "User not found");

            if (user.Password != dto.Password)
                return (false, null, null, "Invalid password");


            var token = _jwt.GenerateToken(user.Username!, user.Role!);

            var userRes = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username!,
                Email = user.Email!,
                Role = user.Role!,
                AvatarUrl = user.AvatarUrl
            };

            return (true, token, userRes, "Login successful");
        }
    }
}
