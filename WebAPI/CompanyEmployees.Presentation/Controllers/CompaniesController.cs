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
        public IActionResult GetCompanies()
        {
            var result = _serviceManager.CompanyService.GetAllCompanies(trackChanges: false);
            return Ok(result);
        }

        [HttpGet("{companyId:guid}", Name = "CompanyById")]
        public IActionResult GetCompany(Guid companyId)
        {
            var result = _serviceManager.CompanyService.GetCompany(companyId, false);

            return Ok(result);
        }

        [HttpPost]
        public IActionResult CreateCompany([FromBody] CompanyForCreationDto company)
        {
            if (company == null)
                return BadRequest("CompanyForCreateionDto object is null");

            var createdCompany = _serviceManager.CompanyService.CreateCompany(company);

            return CreatedAtRoute("CompanyById", new { companyId = createdCompany.Id }, createdCompany);
        }

        [HttpGet("collection/{ids}", Name = "CompanyCollection")]
        public IActionResult GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            var companies = _serviceManager.CompanyService.GetByIds(ids, false);

            return Ok(companies);
        }

        [HttpPost("collection")]
        public IActionResult CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companies)
        {
            var result = _serviceManager.CompanyService.CreateCompanyCollection(companies);

            return CreatedAtRoute("CompanyCollection", new { result.ids }, result.companies);
        }

        [HttpDelete("{companyId:guid}")]
        public IActionResult DeleteCompany(Guid companyId)
        {
            _serviceManager.CompanyService.DeleteCompany(companyId, false);

            return NoContent();
        }

        [HttpPut("{companyId:guid}")]
        public IActionResult UpdateCompany(Guid companyId, CompanyForUpdateDto companyDto)
        {
            _serviceManager.CompanyService.UpdateCompany(companyId, companyDto, true);

            return NoContent();
        }
    }
}
