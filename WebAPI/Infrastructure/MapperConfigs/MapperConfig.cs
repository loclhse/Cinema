using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MapperConfigs
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            MappingUser();
        }

        public void MappingUser()
        {
            CreateMap<AppUser,ReadEmployeeAccount>().ReverseMap();
            CreateMap<AppUser,WriteEmloyeeAccount>().ReverseMap();
            CreateMap<MemberUpdateResquest, AppUser>().ReverseMap();
            CreateMap<EmployeeUpdateResquest, AppUser>().ReverseMap();
            CreateMap<AppUser, MemberResponse>().ReverseMap();
            CreateMap<AppUser, EmployeeResponse>().ReverseMap();
        }
    }
}
