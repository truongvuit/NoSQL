using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Common
{
    public class CompanyDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string EmployeeSize { get; set; }
        public string BusinessField { get; set; }
        public string Introduction { get; set; }
        public CompanyLocationDto Location { get; set; }
        public bool Verified { get; set; }
    }

    public class CompanyLocationDto
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
    }
}
