using Application.DTOs.DtoRequest;
using Application.DTOs.DtoResponse;
using AutoMapper;
using Infrastructure.Entities;

namespace Infrastructure.MapperConfigs
    {
        public class MapperConfig : Profile
        {
            public MapperConfig()
            {
                MappingUser();
            }

            private void MappingUser()
            {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.IdentityCard, opt => opt.MapFrom(src => src.Identitycart))
                .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => src.Dob))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex))    
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.role))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate));

            // Ánh xạ từ UserUpdateDto sang User (chỉ các trường cần cập nhật)
            CreateMap<UserUpdateRequest, User>()
                  .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                 
                  .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                  .ForMember(dest => dest.Identitycart, opt => opt.MapFrom(src => src.IdentityCard))
                  .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => src.Dob))
                  .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                  .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                  .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                  .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex))
                 .ForMember(dest => dest.Id, opt => opt.Ignore())
                 .ForMember(dest => dest.Password, opt => opt.Ignore())
    .ForMember(dest => dest.role, opt => opt.Ignore())
    .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
    .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<User, UserUpdateResponse>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))

                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.IdentityCard, opt => opt.MapFrom(src => src.Identitycart))
                .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => src.Dob))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate));
            
        }
    }
}
        
    
