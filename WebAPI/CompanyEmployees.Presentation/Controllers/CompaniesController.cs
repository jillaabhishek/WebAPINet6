using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public CompaniesController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var result = await _serviceManager.CompanyService.GetAllCompaniesAsync(trackChanges: false);
            return Ok(result);
        }

        [HttpGet("{companyId:guid}", Name = "CompanyById")]
        public async Task<IActionResult> GetCompany(Guid companyId)
        {
            var result = await _serviceManager.CompanyService.GetCompanyAsync(companyId, false);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {
            if (company == null)
                return BadRequest("CompanyForCreateionDto object is null");

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            var createdCompany = await _serviceManager.CompanyService.CreateCompanyAsync(company);

            return CreatedAtRoute("CompanyById", new { companyId = createdCompany.Id }, createdCompany);
        }

        [HttpGet("collection/{ids}", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            var companies = await _serviceManager.CompanyService.GetByIdsAsync(ids, false);

            return Ok(companies);
        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companies)
        {
            var result = await _serviceManager.CompanyService.CreateCompanyCollectionAsync(companies);

            return CreatedAtRoute("CompanyCollection", new { result.ids }, result.companies);
        }

        [HttpDelete("{companyId:guid}")]
        public async Task<IActionResult> DeleteCompany(Guid companyId)
        {
            await _serviceManager.CompanyService.DeleteCompanyAsync(companyId, false);

            return NoContent();
        }

        [HttpPut("{companyId:guid}")]
        public async Task<IActionResult> UpdateCompany(Guid companyId, CompanyForUpdateDto companyDto)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            await _serviceManager.CompanyService.UpdateCompanyAsync(companyId, companyDto, true);

            return NoContent();
        }
    }
}
