using AutoMapper;
using Entities.Models;
using Shared.DataTransferObjects;

namespace WebAPI
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDto>()
                    .ForCtorParam("FullAddress", opt => opt.MapFrom(y => string.Join(y.Address, y.Country)))
                    .ReverseMap();                
        }
    }
}
