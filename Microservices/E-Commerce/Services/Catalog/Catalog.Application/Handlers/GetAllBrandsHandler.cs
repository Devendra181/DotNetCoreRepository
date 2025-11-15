//using AutoMapper;
using Catalog.Application.Mappers;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using MediatR;
using System.Net.Http.Headers;

namespace Catalog.Application.Handlers
{
    public class GetAllBrandsHandler : IRequestHandler<GetAllBrandsQuery, IList<BrandResponse>>
    {
        private readonly IBrandRepository _brandRepository;
        //public IMapper _mapper { get; }

        public GetAllBrandsHandler(IBrandRepository brandRepository/*, IMapper mapper*/)
        {
            _brandRepository = brandRepository;
            //_mapper = mapper;
        }


        public  async Task<IList<BrandResponse>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
        {
            var brandList = await _brandRepository.GetAllBrands();
            //var brandResponseList = _mapper.Map<IList<BrandResponse>>(brandList);
            //var brandResponseList = _mapper.Map<IList<ProductBrand>,IList<BrandResponse>>(brandList.ToList());
            var brandResponseList = ProductMapper.Mapper.Map<IList<ProductBrand>, IList<BrandResponse>>(brandList.ToList());

            return brandResponseList;
        }
    }
}
