using CompanyEmployees.Presentation.Extensions;
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
    [ApiVersion("2.0", Deprecated = true)] // This is using Microsoft.AspNetCore.Mvc.Versioning library
    [Route("api/companies")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v2")]
    public class CompaniesV2Controller : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public CompaniesV2Controller(IServiceManager service) => _serviceManager = service;

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var baseResult = await _serviceManager.CompanyService.GetAllCompaniesAsync(trackChanges: false);
            var companies = baseResult.GetResult<IEnumerable<CompanyDto>>();

            return Ok(companies);
        }

    }
}
