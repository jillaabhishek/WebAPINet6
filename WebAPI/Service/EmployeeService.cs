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
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;

        public EmployeeService(IRepositoryManager repositoryManager, 
                               ILoggerManager loggerManager, 
                               IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }

        public async Task<EmployeeDto> CreateEmployeeForCompanyAsync(Guid companyId, EmployeeForCreationDto employee, bool trackChanges)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges);
            if (company == null)
                throw new CompanyNotFoundException(companyId);

            var employeeEntity = _mapper.Map<Employee>(employee);

            _repositoryManager.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repositoryManager.SaveASync();

            return _mapper.Map<EmployeeDto>(employeeEntity);
        }

        public async Task<EmployeeDto> GetEmployeeAsync(Guid companyId, Guid employeeId, bool trackChanges)
        {
            var company = await _repositoryManager.Company.GetCompany(companyId, trackChanges);

            if (company == null)
                throw new CompanyNotFoundException(companyId);

            var employee = await _repositoryManager.Employee.GetEmployee(companyId, employeeId, trackChanges);

            if (employee == null)
                throw new EmployeeNotFoundException(employeeId);

            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(Guid companyId, bool trackChanges)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges);

            if (company == null)
                throw new CompanyNotFoundException(companyId);
            
            var employees = await _repositoryManager.Employee.GetEmployees(companyId, trackChanges);

            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid employeeId, bool trackChanges)
        {
            var company = await _repositoryManager.Company.GetCompany(companyId, trackChanges);

            if (company == null) throw new CompanyNotFoundException(companyId);

            var employee = await _repositoryManager.Employee.GetEmployee(companyId, employeeId, trackChanges);

            if (employee == null) throw new EmployeeNotFoundException(employeeId);

            _repositoryManager.Employee.DeleteEmployee(employee);
            await _repositoryManager.SaveASync();
        }

        public async Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdateDto, 
                                            bool compTrackChanges, bool empTrackChanges)
        {
            var company = await _repositoryManager.Company.GetCompany(companyId, compTrackChanges);
            if (company == null) throw new CompanyNotFoundException(companyId);

            var employee = await _repositoryManager.Employee.GetEmployee(companyId, id, empTrackChanges);
            if (employee == null) throw new EmployeeNotFoundException(id);

            _mapper.Map(employeeForUpdateDto, employee);
            await _repositoryManager.SaveASync();
        }

        public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPathAsync(Guid companyId, Guid id, 
                                bool compTrackChanges, bool empTrackChanges)
        {
            var company = await _repositoryManager.Company.GetCompany(companyId, compTrackChanges);
            if (company == null) throw new CompanyNotFoundException(companyId);

            var employee = await _repositoryManager.Employee.GetEmployee(companyId, id, empTrackChanges);
            if (employee == null) throw new EmployeeNotFoundException(id);

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employee);

            return (employeeToPatch, employee);
        }

        public async Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeForUpdateDto, Employee employeeEntity)
        {
            _mapper.Map(employeeForUpdateDto, employeeEntity);
            await _repositoryManager.SaveASync();
        }
    }
}
