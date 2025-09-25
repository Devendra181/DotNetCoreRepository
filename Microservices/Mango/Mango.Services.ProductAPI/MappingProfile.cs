using AutoMapper;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;


namespace Mango.Services.ProductAPI
{
    public class MappingProfile : Profile
    {
        //public static MapperConfiguration RegisterMaps()
        //{
        //    var mappingConfig = new MapperConfiguration(config =>
        //    {
        //        config.CreateMap<ProductDto, Product>();
        //        config.CreateMap<Coupon, CouponDto>();
        //    });
        //    return mappingConfig;
        //}
        public MappingProfile()
        {
            CreateMap<ProductDto, Product>().ReverseMap();
        }
    }
}