using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
    public abstract record CompanyForManipulationDto
    {
        [Required(ErrorMessage = "Company Name is Required.")]
        [MaxLength(30, ErrorMessage = "Maximum length for the Name is 30 characters.")]
        public string? Name { get; init; }
        [Required(ErrorMessage = "Company Address is Required")]
        public string? Address { get; init; }
        [Required(ErrorMessage = "Company Country is Required")]
        public string? Country { get; init; }
        public IEnumerable<EmployeeForCreationDto>? Employees { get; init; }
    }
}
