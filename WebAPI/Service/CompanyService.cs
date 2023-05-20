using AutoMapper;
using Contracts;
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

        public IEnumerable<CompanyDto> GetAllCompanies(bool trackChanges)
        {
            try
            {
                var companies = _repositoryManager
                                    .Company
                                    .GetAllCompanies(trackChanges);

                return _mapper.Map<IEnumerable<CompanyDto>>(companies); ;
            }
            catch (Exception ex)
            {
                _logggerManager.LogError($"Something went wrong in the {nameof(GetAllCompanies)} service method {ex}");
                throw;
            }
        }
    }
}
