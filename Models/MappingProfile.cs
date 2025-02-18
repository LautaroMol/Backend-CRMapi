using AutoMapper;
using CRMapi.DTOs;
using CRMapi.Models.Entity;

namespace CRMapi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductDTO, Product>();
            CreateMap<ClientsDTO, Clients>();
            CreateMap<OrderDetailsDTO, OrderDetails>();
            CreateMap<PersonalDTO, Personal>();
            CreateMap<OrdersDTO, Orders>();
            CreateMap<OrdersDTO, Orders>()
            .ForMember(dest => dest.Client, opt => opt.Ignore());

        }
    }
}
