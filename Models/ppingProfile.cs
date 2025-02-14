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
            CreateMap<OrdersDTO, Orders>();
            CreateMap<OrderDetailsDTO, OrderDetails>();
            CreateMap<Product, ProductDTO>();
            CreateMap<Clients, ClientsDTO>();
        }
    }
}
