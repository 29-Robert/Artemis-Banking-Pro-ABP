using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Application.Features.Users.Commands.CreateUser;
using ArtemisBankingPro.Domain.Entities;
using AutoMapper;

namespace ArtemisBankingPro.Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<CreateUserCommand, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<User, UserDto>();
        }
    }
}