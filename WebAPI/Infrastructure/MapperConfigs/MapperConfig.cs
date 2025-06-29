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
            CreateMap<CinemaRoom, CinemaRoomCreateRequest>().ReverseMap();
            CreateMap<CinemaRoom, CinemaRoomUpdateRequest>()
                .ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CinemaRoom, CinemaRoomResponse>().ReverseMap();
            CreateMap<Seat, SeatCreateRequest>().ReverseMap();
            CreateMap<Seat, SeatUpdateRequest>().ReverseMap();
            CreateMap<Seat, SeatResponse>().ReverseMap();
            CreateMap<SeatTypePrice, SeatTypePriceCreateRequest>().ReverseMap();
            CreateMap<SeatTypePrice, SeatTypePriceUpdateRequest>().ReverseMap();
            CreateMap<SeatTypePrice, SeatTypePriceResponse>().ReverseMap();
            CreateMap<Promotion, EditPromotionRequest>().ReverseMap();
            CreateMap<Promotion, PromotionResponse>().ReverseMap();
            CreateMap<Genre, GenreResponse>().ReverseMap();
            CreateMap<GenreRequest, Genre>().ReverseMap();
            CreateMap<Movie,MovieResponse>().ReverseMap();
            CreateMap<MovieRequest, Movie>().ReverseMap();
            CreateMap<Showtime, ShowtimeResponse>().ReverseMap();
            CreateMap<ShowtimeResquest, Showtime>().ReverseMap();
            CreateMap<Showtime, ShowtimeUpdateRequest>().ReverseMap();
            CreateMap<Snack, SnackResponse>().ReverseMap();
            CreateMap<SnackRequest, Snack>().ReverseMap();
           
           
            CreateMap<SnackComboRequest, SnackCombo>().ReverseMap();
            CreateMap<SubscriptionPlanRequest, SubscriptionPlan>().ReverseMap();
            CreateMap<SubscriptionPlan, SubscriptionPlanResponse>().ReverseMap();
            CreateMap<Showtime, MovieTimeResponse>().ReverseMap();
           

           
            CreateMap<SnackComboUpdateRequest, SnackCombo>().ReverseMap();
            CreateMap<SnackComboItem, SnackComboItemDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SnackId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Snack.Name))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.ImgUrl, opt => opt.MapFrom(src => src.Snack.imgUrl))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Snack.Price))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Snack.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Snack.Description));



            CreateMap<SnackCombo, SnackComboResponse>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.SnackComboItems))
                .ReverseMap();


            CreateMap<SeatSchedule, SeatScheduleResponse>()
                .ForMember(dest => dest.IsOwnedByCaller, opt => opt.Ignore())
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Seat != null ? src.Seat.Label : string.Empty))
                .ReverseMap();

            CreateMap<Showtime, RoomShowtimeResponse>().ReverseMap();
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

            
            CreateMap<Payment, PaymentResponse>().ReverseMap();



        }
    }
}
