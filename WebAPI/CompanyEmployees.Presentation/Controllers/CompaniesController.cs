using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Presentation.Extensions;
using Entities.Responses;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
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
    [ApiExplorerSettings(GroupName = "v1")]
    //[ResponseCache(CacheProfileName = "120SecondDuration")]
    public class CompaniesController : ApiControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public CompaniesController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        /// <summary>
        /// Gets the list of all companies
        /// </summary>
        /// <returns>The companies list</returns>
        [HttpGet(Name = "GetCompanies")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetCompanies()
        {
            var baseResult = await _serviceManager.CompanyService.GetAllCompaniesAsync(trackChanges: false);
            var companies = baseResult.GetResult<IEnumerable<CompanyDto>>();

            return Ok(companies);
        }

        [HttpGet("{companyId:guid}", Name = "CompanyById")]
        //[ResponseCache(Duration = 60)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)]
        [HttpCacheValidation(MustRevalidate = false)]
        [Authorize]
        public async Task<IActionResult> GetCompany(Guid companyId)
        {
            var baseResult = await _serviceManager.CompanyService.GetCompanyAsync(companyId, false);

            if (!baseResult.Success)
                return ProcessError(baseResult);

            var company = baseResult.GetResult<CompanyDto>();

            return Ok(company);
        }

        [HttpPost(Name = "CreateCompany")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {
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
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid companyId, CompanyForUpdateDto companyDto)
        {
            await _serviceManager.CompanyService.UpdateCompanyAsync(companyId, companyDto, true);

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "GET, POST, DELETE, OPTIONS, PUT");

            return Ok();
        }
    }
}
