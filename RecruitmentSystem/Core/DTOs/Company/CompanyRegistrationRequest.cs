using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Company
{
    public class CompanyRegistrationRequest
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Website { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Phone { get; set; }
        
        [Required]
        public string EmployeeSize { get; set; }
        
        [Required]
        public string BusinessField { get; set; }
        
        public string TaxCode { get; set; }
        
        public int? FoundedYear { get; set; }
        
        [Required]
        public string Introduction { get; set; }
        
        public string Vision { get; set; }
        
        public string Mission { get; set; }
        
        public List<string> CoreValues { get; set; }
        
        public CompanyLocationDto Location { get; set; }
        
        public string LogoUrl { get; set; }
    }

    public class CompanyLocationDto
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Country { get; set; } = "Việt Nam";
    }

    public class CompanyRegistrationResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Verified { get; set; }
        public string Status { get; set; } = "pending";
    }

    public class CompanyVerificationRequest
    {
        [Required]
        public string CompanyId { get; set; }
        
        [Required]
        public bool Approve { get; set; }
        
        public string? RejectionReason { get; set; }
    }

    public class PendingCompanyDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BusinessField { get; set; }
        public string TaxCode { get; set; }
        public string LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserCompanyDto RequestedBy { get; set; }
    }

    public class UserCompanyDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
    }
}