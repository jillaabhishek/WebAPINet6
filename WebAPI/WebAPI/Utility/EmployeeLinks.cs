using Contracts;
using Entities.LinkModels;
using Entities.Models;
using Microsoft.Net.Http.Headers;
using Shared.DataTransferObjects;
using System.ComponentModel.Design;

namespace WebAPI.Utility
{
    public class EmployeeLinks : IEmployeeLinks
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<EmployeeDto> _dataShaper;

        public EmployeeLinks(LinkGenerator linkGenerator, IDataShaper<EmployeeDto> dataShaper)
        {
            _linkGenerator = linkGenerator;
            _dataShaper = dataShaper;
        }

        public LinkResponse TryGenerateLinks(IEnumerable<EmployeeDto> employeesDto, string fields,
                                                        Guid companyId, HttpContext httpContext)
        {
            List<Entity> shapedEmployees = ShapeData(employeesDto, fields);

            if (ShouldGenerateLinks(httpContext))
                return ReturnLinkedEmployees(employeesDto, fields, companyId, httpContext, shapedEmployees);

            return ReturnShapedEmployees(shapedEmployees);
        }

        private List<Entity> ShapeData(IEnumerable<EmployeeDto> employeesDto, string fields) =>
                _dataShaper.ShapeData(employeesDto, fields)
                           .Select(x => x.Entity)
                           .ToList();

        private bool ShouldGenerateLinks(HttpContext httpContext)
        {
            var mediaType = (httpContext.Items["AcceptHeaderMediaType"]);

            return ((MediaTypeHeaderValue)mediaType).SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);
        }

        private LinkResponse ReturnShapedEmployees(List<Entity> shapedEmployees) => new LinkResponse { ShapedEntities = shapedEmployees };

        private LinkResponse ReturnLinkedEmployees(IEnumerable<EmployeeDto> employeeDtos, string fields,
                                                   Guid companyId, HttpContext httpContext,
                                                   List<Entity> shapedEmployees)
        {
            var employeeDtoList = employeeDtos.ToList();

            for (var index = 0; index < employeeDtos.Count(); index++)
            {
                var employeeLinks = CreateLinksForEmployee(httpContext, companyId, employeeDtoList[index].Id, fields);
                shapedEmployees[index].Add("Links", employeeLinks);
            }

            var employeeCollection = new LinkCollectionWrapper<Entity>(shapedEmployees);
            var linkedEmployees = CreateLinksForEmployees(httpContext, employeeCollection);

            return new LinkResponse { HasLinks = true, LinkedEntities = linkedEmployees };
        }

        private List<Link> CreateLinksForEmployee(HttpContext httpContext, Guid companyId, Guid id, string fields = "")
        {
            var links = new List<Link>
            {
                new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployees", values: new {companyId, id}), 
                        "self", "GET"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "DeleteEmployee", values: new {companyId, id}), 
                        "delete_employee", "DELETE"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "UpdateEmployeeForCompany",  values: new {companyId, id}), 
                        "update_employee", "PUT"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "PartiallyUpdateEmployeeForCompany", values: new {companyId, id}),                 "partially_update_employee", "PATCH"),
            };

            return links;
        }

        private LinkCollectionWrapper<Entity> CreateLinksForEmployees(HttpContext httpContext, LinkCollectionWrapper<Entity> employeeWrapper)
        {
            employeeWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployees", values: new { }), "self", "GET"));

            return employeeWrapper;
        }
    }
}
