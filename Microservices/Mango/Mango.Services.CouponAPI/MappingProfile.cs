using AutoMapper;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.Dto;

namespace Mango.Services.CouponAPI
{
    public class MappingProfile : Profile
    {
        //public static MapperConfiguration RegisterMaps()
        //{
        //    var mappingConfig = new MapperConfiguration(config =>
        //    {
        //        config.CreateMap<CouponDto, Coupon>();
        //        config.CreateMap<Coupon, CouponDto>();
        //    });
        //    return mappingConfig;
        //}
        public MappingProfile()
        {
            CreateMap<CouponDto, Coupon>().ReverseMap();
        }
    }
}