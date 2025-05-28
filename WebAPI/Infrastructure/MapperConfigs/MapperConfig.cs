using AutoMapper;

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
            // Example mapping for User entity
            // CreateMap<User, UserDto>().ReverseMap;
        }
    }
}
