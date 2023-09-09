using CompanyEmployees.Presentation.ActionFilters;
using Entities.LinkModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
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
        [HttpHead]
        [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            var linkParams = new LinkParameters(employeeParameters, HttpContext);

            var result = await _servicesManager.EmployeeService.GetEmployeesAsync(companyId, linkParams, false);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.metadata));

            return result.linkResponse.HasLinks ? 
                    Ok(result.linkResponse.LinkedEntities) : 
                    Ok(result.linkResponse.ShapedEntities);
        }

        [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployees(Guid companyId, Guid id)
        {
            var employee = await _servicesManager
                                .EmployeeService
                                .GetEmployeeAsync(companyId, id, false);

            return Ok(employee);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, EmployeeForCreationDto employee)
        {
            var createdEmployee = await _servicesManager.EmployeeService.CreateEmployeeForCompanyAsync(companyId, employee, false);

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, employeeId = createdEmployee.Id }, createdEmployee);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid companyId, Guid id)
        {
            await _servicesManager.EmployeeService.DeleteEmployeeForCompanyAsync(companyId, id, false);

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        [ServiceFilter(typeof (ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, 
                    [FromBody] EmployeeForUpdateDto employeeDto)
        {
            await _servicesManager.EmployeeService.UpdateEmployeeForCompanyAsync(companyId, id, employeeDto, false, true);

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
