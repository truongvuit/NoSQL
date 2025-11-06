using AutoMapper;
using Core.DTOs.Admin;
using Core.DTOs.User;
using Core.DTOs.Common;
using Core.Models;

namespace API
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Base mapping
            CreateMap<User, BaseUserDto>();
            
            // User mappings
            CreateMap<User, UserSummaryDto>()
                .IncludeBase<User, BaseUserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Profile.FullName));
            
            CreateMap<User, UserDto>()
                .IncludeBase<User, BaseUserDto>()
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Company != null ? src.Company.Id : null));

            // Nested object mappings
            CreateMap<UserProfile, UserProfileDto>();
            CreateMap<Address, AddressDto>();
            
            // Company mapping
            CreateMap<Company, CompanyDto>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));
            CreateMap<CompanyLocation, CompanyLocationDto>();
        }
    }
}
