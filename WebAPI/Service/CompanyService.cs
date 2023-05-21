using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal sealed class CompanyService : ICompanyService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _logggerManager;
        private readonly IMapper _mapper;

        public CompanyService(IRepositoryManager repositoryManager, ILoggerManager logggerManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _logggerManager = logggerManager;
            _mapper = mapper;
        }

        public CompanyDto CreateCompany(CompanyForCreationDto company)
        {
            var companyEntity = _mapper.Map<Company>(company);

            _repositoryManager.Company.CreateCompany(companyEntity);
            _repositoryManager.Save();

            return _mapper.Map<CompanyDto>(companyEntity);
        }

        public (IEnumerable<CompanyDto> companies, string ids) CreateCompanyCollection(IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if(companyCollection == null)           
                throw new CompanyCollectionBadRequest();

            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);

            foreach(var companyEntity in companyEntities)
            {
                _repositoryManager.Company.CreateCompany(companyEntity);
            }

            _repositoryManager.Save();

            string ids = String.Join(",", companyEntities.Select(x => x.Id));

            return (_mapper.Map<IEnumerable<CompanyDto>>(companyEntities), ids);            
        }

        public IEnumerable<CompanyDto> GetAllCompanies(bool trackChanges)
        {
            var companies = _repositoryManager
                                .Company
                                .GetAllCompanies(trackChanges);

            return _mapper.Map<IEnumerable<CompanyDto>>(companies);
        }

        public IEnumerable<CompanyDto> GetByIds(IEnumerable<Guid> ids, bool trackChanges)
        {
            if (ids is null)
                throw new IdParametersBadRequestExceptions();

            var comapanyEntities = _repositoryManager.Company.GetByIds(ids, trackChanges);
            if (ids.Count() != comapanyEntities.Count())
                throw new CollectionByIdsBadRequestException();

            return _mapper.Map<IEnumerable<CompanyDto>>(comapanyEntities);
        }

        public CompanyDto GetCompany(Guid companyId, bool trackChanges)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges);

            if(company == null)            
                throw new CompanyNotFoundException(companyId);            

            return _mapper.Map<CompanyDto>(company);
        }        
    }
}
