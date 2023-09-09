using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.LinkModels;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;
        private readonly IEmployeeLinks _employeeLinks;

        public EmployeeService(IRepositoryManager repositoryManager,
                               ILoggerManager loggerManager,
                               IMapper mapper,
                               IDataShaper<EmployeeDto> dataShaper,
                               IEmployeeLinks employeeLinks)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
            _mapper = mapper;
            _dataShaper = dataShaper;
            _employeeLinks = employeeLinks;
        }

        public async Task<EmployeeDto> CreateEmployeeForCompanyAsync(Guid companyId, EmployeeForCreationDto employee, bool trackChanges)
        {
            await CheckIfCompanyExist(companyId, trackChanges);

            var employeeEntity = _mapper.Map<Employee>(employee);

            _repositoryManager.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repositoryManager.SaveASync();

            return _mapper.Map<EmployeeDto>(employeeEntity);
        }

        public async Task<EmployeeDto> GetEmployeeAsync(Guid companyId, Guid employeeId, bool trackChanges)
        {
            await CheckIfCompanyExist(companyId, trackChanges);
            var employee = await GetEmployeeForCompanyAndCheckIfItExists(companyId, employeeId, trackChanges);

            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<(LinkResponse linkResponse, MetaData metadata)> GetEmployeesAsync(Guid companyId,
                            LinkParameters linkParams, bool trackChanges)
        {
            if (!linkParams.EmployeeParameters.ValidAgeRange)
                throw new MaxAgeRangeBadRequestException();

            await CheckIfCompanyExist(companyId, trackChanges);

            var employeesWithMetadata = await _repositoryManager.Employee.GetEmployees(companyId, linkParams.EmployeeParameters, trackChanges);

            var employeeDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesWithMetadata);
            var links = _employeeLinks.TryGenerateLinks(employeeDto, linkParams.EmployeeParameters.Fields, companyId, linkParams.Context);

            return (links, employeesWithMetadata.MetaData);
        }

        public async Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid employeeId, bool trackChanges)
        {
            await CheckIfCompanyExist(companyId, trackChanges);

            var employee = await GetEmployeeForCompanyAndCheckIfItExists(companyId, employeeId, trackChanges);

            _repositoryManager.Employee.DeleteEmployee(employee);
            await _repositoryManager.SaveASync();
        }

        public async Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdateDto,
                                            bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExist(companyId, compTrackChanges);

            var employee = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

            _mapper.Map(employeeForUpdateDto, employee);
            await _repositoryManager.SaveASync();
        }

        public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPathAsync(Guid companyId, Guid id,
                                bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExist(companyId, compTrackChanges);
            var employee = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employee);

            return (employeeToPatch, employee);
        }

        public async Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeForUpdateDto, Employee employeeEntity)
        {
            _mapper.Map(employeeForUpdateDto, employeeEntity);
            await _repositoryManager.SaveASync();
        }

        public async Task<Company> CheckIfCompanyExist(Guid companyId, bool trackChanges)
        {
            var company = await _repositoryManager.Company.GetCompany(companyId, trackChanges);
            if (company == null) throw new CompanyNotFoundException(companyId);

            return company;
        }

        public async Task<Employee> GetEmployeeForCompanyAndCheckIfItExists(Guid companyId, Guid id, bool trackChanges)
        {
            var employee = await _repositoryManager.Employee.GetEmployee(companyId, id, trackChanges);
            if (employee == null) throw new EmployeeNotFoundException(id);

            return employee;
        }
    }
}
