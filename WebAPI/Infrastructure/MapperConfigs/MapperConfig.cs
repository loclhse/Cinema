using Application.ViewModel;
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
        }
    }
}
