using System;

namespace ArtemisBankingPro.Application.DTOs.Users
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? CommerceId { get; set; }
        public DateTime Expiration { get; set; }
    }
}
