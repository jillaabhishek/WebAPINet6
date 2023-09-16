using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Entities.Responses;
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

        public async Task<ApiBaseResponse> GetAllCompaniesAsync(bool trackChanges)
        {
            var companies = await _repositoryManager
                                .Company
                                .GetAllCompanies(trackChanges);
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            return new ApiOkResponse<IEnumerable<CompanyDto>>(companiesDto);
        }

        public async Task<ApiBaseResponse> GetCompanyAsync(Guid companyId, bool trackChanges)
        {
            var company = await _repositoryManager.Company.GetCompany(companyId, trackChanges);

            if (company == null) return new CompanyNotFoundResponse(companyId);

            var companyDto = _mapper.Map<CompanyDto>(company);
            return new ApiOkResponse<CompanyDto>(companyDto);
        }

        public async Task<CompanyDto> CreateCompanyAsync(CompanyForCreationDto company)
        {
            var companyEntity = _mapper.Map<Company>(company);

            _repositoryManager.Company.CreateCompany(companyEntity);
            await _repositoryManager.SaveASync();

            return _mapper.Map<CompanyDto>(companyEntity);
        }

        public async Task<(IEnumerable<CompanyDto> companies, string ids)> CreateCompanyCollectionAsync(IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if (companyCollection == null)
                throw new CompanyCollectionBadRequest();

            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);

            foreach (var companyEntity in companyEntities)
            {
                _repositoryManager.Company.CreateCompany(companyEntity);
            }

            await _repositoryManager.SaveASync();

            string ids = String.Join(",", companyEntities.Select(x => x.Id));

            return (_mapper.Map<IEnumerable<CompanyDto>>(companyEntities), ids);
        }

        public async Task<IEnumerable<CompanyDto>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
        {
            if (ids is null)
                throw new IdParametersBadRequestExceptions();

            var comapanyEntities = await _repositoryManager.Company.GetByIds(ids, trackChanges);
            if (ids.Count() != comapanyEntities.Count())
                throw new CollectionByIdsBadRequestException();

            return _mapper.Map<IEnumerable<CompanyDto>>(comapanyEntities);
        }

        public async Task DeleteCompanyAsync(Guid companyId, bool trackChanges)
        {
            var company = await GetCompanyAndCheckIfItExist(companyId, trackChanges);

            _repositoryManager.Company.DeleteCompany(company);
            await _repositoryManager.SaveASync();
        }

        public async Task UpdateCompanyAsync(Guid companyId, CompanyForUpdateDto companyDto, bool trackChanges)
        {
            var company = await GetCompanyAndCheckIfItExist(companyId, trackChanges);

            _mapper.Map(companyDto, company);
            await _repositoryManager.SaveASync();
        }

        private async Task<Company> GetCompanyAndCheckIfItExist(Guid companyId, bool trackChanges)
        {
            var company = await _repositoryManager.Company.GetCompany(companyId, trackChanges);

            if (company == null) throw new CompanyNotFoundException(companyId);

            return company;
        }
    }
}
