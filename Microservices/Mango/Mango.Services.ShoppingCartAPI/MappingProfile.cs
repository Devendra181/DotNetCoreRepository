using AutoMapper;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;


namespace Mango.Services.ShoppingCartAPI
{
    public class MappingProfile : Profile
    {
        //var mappingConfig = new MapperConfiguration(config =>
        //{
        //    config.CreateMap<CartHeader, CartHeaderDto>().ReverseMap();
        //    config.CreateMap<CartDetails, CartDetailsDto>().ReverseMap();
        //});
        //    return mappingConfig;
        public MappingProfile()
        {
            CreateMap<CartHeader, CartHeaderDto>().ReverseMap();
            CreateMap<CartDetails, CartDetailsDto>().ReverseMap();
        }
    }
}