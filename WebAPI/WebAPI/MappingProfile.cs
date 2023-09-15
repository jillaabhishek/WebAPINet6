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
                    .ForMember(x => x.FullAddress, opt => opt.MapFrom(y => string.Join(' ', y.Address, y.Country)))
                    .ReverseMap();

            CreateMap<Employee, EmployeeDto>().ReverseMap();

            CreateMap<Company, CompanyForCreationDto>().ReverseMap();

            CreateMap<Employee, EmployeeForCreationDto>().ReverseMap();

            CreateMap<Employee, EmployeeForUpdateDto>().ReverseMap();

            CreateMap<Company, CompanyForUpdateDto>().ReverseMap();

            CreateMap<UserForRegistrationDto, User>().ReverseMap();
        }
    }
}
