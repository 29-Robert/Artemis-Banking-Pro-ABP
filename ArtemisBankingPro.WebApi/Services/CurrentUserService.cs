using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApi.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                               ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return int.TryParse(userIdClaim, out int userId) ? userId : 0;
            }
        }

        public string Role
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            }
        }

        public int? CommerceId
        {
            get
            {
                var commerceIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("CommerceId")?.Value;
                if (!string.IsNullOrEmpty(commerceIdClaim) && int.TryParse(commerceIdClaim, out int commerceId))
                {
                    return commerceId;
                }
                return null;
            }
        }
    }
}
