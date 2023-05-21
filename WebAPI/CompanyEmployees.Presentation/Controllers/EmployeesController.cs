using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IServiceManager _servicesManager;
        public EmployeesController(IServiceManager serviceManager)
        {
            _servicesManager = serviceManager;
        }

        [HttpGet]
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            var employees = _servicesManager.EmployeeService.GetEmployees(companyId, false);

            return Ok(employees);
        }

        [HttpGet("{employeeId:guid}", Name = "GetEmployeeForCompany")]
        public IActionResult GetEmployees(Guid companyId, Guid employeeId)
        {
            var employee = _servicesManager
                                .EmployeeService
                                .GetEmployee(companyId, employeeId, false);

            return Ok(employee);
        }

        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, EmployeeForCreationDto employee)
        {
            if (employee == null)
                return BadRequest($"EmployeeForCreationDto object is null");

            var createdEmployee = _servicesManager.EmployeeService.CreateEmployeeForCompany(companyId, employee, false);

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, employeeId = createdEmployee.Id }, createdEmployee);
        }
    }
}
