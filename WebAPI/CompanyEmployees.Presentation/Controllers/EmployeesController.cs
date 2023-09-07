using CompanyEmployees.Presentation.ActionFilters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            var pagedResult = await _servicesManager.EmployeeService.GetEmployeesAsync(companyId, employeeParameters, false);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagedResult.metadata));

            return Ok(pagedResult.employees);
        }

        [HttpGet("{employeeId:guid}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployees(Guid companyId, Guid employeeId)
        {
            var employee = await _servicesManager
                                .EmployeeService
                                .GetEmployeeAsync(companyId, employeeId, false);

            return Ok(employee);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, EmployeeForCreationDto employee)
        {
            var createdEmployee = await _servicesManager.EmployeeService.CreateEmployeeForCompanyAsync(companyId, employee, false);

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, employeeId = createdEmployee.Id }, createdEmployee);
        }

        [HttpDelete("{employeeId:guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid companyId, Guid employeeId)
        {
            await _servicesManager.EmployeeService.DeleteEmployeeForCompanyAsync(companyId, employeeId, false);

            return NoContent();
        }

        [HttpPut("{employeeId:guid}")]
        [ServiceFilter(typeof (ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid employeeId, 
                    [FromBody] EmployeeForUpdateDto employeeDto)
        {
            await _servicesManager.EmployeeService.UpdateEmployeeForCompanyAsync(companyId, employeeId, employeeDto, false, true);

            return NoContent();
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, 
                        [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDto)
        {
            if (patchDto is null)
                return BadRequest("patchDoc object sent from client is null.");

            var result = await _servicesManager.EmployeeService.GetEmployeeForPathAsync(companyId, id, false, true);

            patchDto.ApplyTo(result.employeeToPatch, ModelState);

            TryValidateModel(result.employeeToPatch);

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            await _servicesManager.EmployeeService.SaveChangesForPatchAsync(result.employeeToPatch, result.employeeEntity);

            return NoContent();
        }
    }
}
