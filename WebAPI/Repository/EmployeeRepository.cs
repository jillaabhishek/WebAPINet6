using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext repositoryContext)
                        : base(repositoryContext)
        {

        }

        public void CreateEmployeeForCompany(Guid companyId, Employee employee)
        {
            employee.CompanyId = companyId;
            Create(employee);
        }

        public async Task<Employee> GetEmployee(Guid companyId, Guid employeeId, bool trackChanges) =>
            await FindByCondition(x => x.CompanyId.Equals(companyId) && x.Id.Equals(employeeId), trackChanges)
                   .SingleOrDefaultAsync();



        public async Task<PagedList<Employee>> GetEmployees(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
        {
            var employee = await FindByCondition(x => x.CompanyId.Equals(companyId), trackChanges)
                           .FilterEmployee(employeeParameters.MinAge, employeeParameters.MaxAge)
                           .Search(employeeParameters.SearchTerm)
                           .Sort(employeeParameters.OrderBy)
                           .Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize)
                           .Take(employeeParameters.PageSize)
                           .ToListAsync();

            var employeeCount = await FindByCondition(x => x.CompanyId == companyId, false).CountAsync();

            return new PagedList<Employee>(employee, employeeCount, employeeParameters.PageNumber, employeeParameters.PageSize);
        }

        public void DeleteEmployee(Employee employee) => Delete(employee);
    }
}
