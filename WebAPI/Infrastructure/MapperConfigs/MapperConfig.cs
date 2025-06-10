using Application.Domain;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity;

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
            CreateMap<AppUser, ReadEmployeeAccount>().ReverseMap();
            CreateMap<AppUser, WriteEmloyeeAccount>().ReverseMap();
            CreateMap<CustomerUpdateResquest, AppUser>().ReverseMap();
            CreateMap<EmployeeUpdateResquest, AppUser>().ReverseMap();
            CreateMap<AppUser, CustomerResponse>().ReverseMap();
            CreateMap<AppUser, EmployeeResponse>().ReverseMap();
            CreateMap<Promotion, EditPromotionRequest>().ReverseMap();
            CreateMap<Promotion, PromotionResponse>().ReverseMap();

            // map từ ApplicationUser (đã có AppUser) sang DTO
            CreateMap<IdentityWithProfile, EmployeeResponse>()
                // Identity
                .ForMember(d => d.Id, m => m.MapFrom(s => s.Profile.Id))
                .ForMember(d => d.Username, m => m.MapFrom(s => s.Identity.UserName))
                .ForMember(d => d.Email, m => m.MapFrom(s => s.Identity.Email))
                .ForMember(d => d.Phone, m => m.MapFrom(s => s.Identity.Phone))
                // Profile
                .ForMember(d => d.IdentityCard, m => m.MapFrom(s => s.Profile.IdentityCard))
                .ForMember(d => d.Dob, m => m.MapFrom(s => s.Profile.Dob))
                .ForMember(d => d.FullName, m => m.MapFrom(s => s.Profile.FullName))
                .ForMember(d => d.Address, m => m.MapFrom(s => s.Profile.Address))
                .ForMember(d => d.Avatar, m => m.MapFrom(s => s.Profile.Avatar))
                .ForMember(d => d.Sex, m => m.MapFrom(s => s.Profile.Sex))
                .ForMember(d => d.Assign, m => m.MapFrom(s => s.Profile.Assign))
                .ForMember(d => d.Salary, m => m.MapFrom(s => s.Profile.Salary))
                .ForMember(d => d.Position, m => m.MapFrom(s => s.Profile.Position))
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<IdentityWithProfile, CustomerResponse>()
                // Identity
                .ForMember(d => d.Id, m => m.MapFrom(s => s.Profile.Id))
                .ForMember(d => d.Username, m => m.MapFrom(s => s.Identity.UserName))
                .ForMember(d => d.Email, m => m.MapFrom(s => s.Identity.Email))
                .ForMember(d => d.Phone, m => m.MapFrom(s => s.Identity.Phone))
                // Profile
                .ForMember(d => d.IdentityCard, m => m.MapFrom(s => s.Profile.IdentityCard))
                .ForMember(d => d.Dob, m => m.MapFrom(s => s.Profile.Dob))
                .ForMember(d => d.FullName, m => m.MapFrom(s => s.Profile.FullName))
                .ForMember(d => d.Address, m => m.MapFrom(s => s.Profile.Address))
                .ForMember(d => d.Avatar, m => m.MapFrom(s => s.Profile.Avatar))
                .ForMember(d => d.Sex, m => m.MapFrom(s => s.Profile.Sex))
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
