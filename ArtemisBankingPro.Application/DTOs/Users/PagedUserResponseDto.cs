namespace ArtemisBankingPro.Application.DTOs
{
    public class PagedUserResponseDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public List<UserDto> Data { get; set; } = new List<UserDto>();
    }
}
