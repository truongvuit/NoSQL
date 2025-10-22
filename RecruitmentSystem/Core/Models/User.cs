using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; }

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("role")]
        public string Role { get; set; } = "candidate";

        [BsonElement("isVerified")]
        public bool IsVerified { get; set; }

        [BsonElement("emailVerifiedAt")]
        public DateTime? EmailVerifiedAt { get; set; }

        [BsonElement("profile")]
        public UserProfile Profile { get; set; }

        [BsonElement("recruiterProfile")]
        public RecruiterProfile RecruiterProfile { get; set; }

        [BsonElement("candidateProfile")]
        public CandidateProfile CandidateProfile { get; set; }

        [BsonElement("statistics")]
        public UserStatistics Statistics { get; set; }

        [BsonElement("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        [BsonElement("loginAttempts")]
        public int LoginAttempts { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("isBanned")]
        public bool IsBanned { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("deletedAt")]
        public DateTime? DeletedAt { get; set; }

        [BsonElement("schemaVersion")]
        public string SchemaVersion { get; set; } = "1.0";

        [BsonElement("company")]
        public Company Company { get; set; }
    }

    public class UserProfile
    {
        [BsonElement("fullName")]
        public string FullName { get; set; }

        [BsonElement("avatar")]
        public string Avatar { get; set; }

        [BsonElement("gender")]
        public string Gender { get; set; }

        [BsonElement("dateOfBirth")]
        public string DateOfBirth { get; set; }

        [BsonElement("address")]
        public Address Address { get; set; }

        [BsonElement("bio")]
        public string Bio { get; set; }

        [BsonElement("socialLinks")]
        public SocialLinks SocialLinks { get; set; }
    }

    public class Address
    {
        [BsonElement("city")]
        public string City { get; set; }

        [BsonElement("district")]
        public string District { get; set; }

        [BsonElement("street")]
        public string Street { get; set; }
    }

    public class SocialLinks
    {
        [BsonElement("linkedin")]
        public string LinkedIn { get; set; }

        [BsonElement("facebook")]
        public string Facebook { get; set; }
    }

    public class RecruiterProfile
    {
        [BsonElement("position")]
        public string Position { get; set; }

        [BsonElement("department")]
        public string Department { get; set; }

        [BsonElement("employeeId")]
        public string EmployeeId { get; set; }

        [BsonElement("joinedDate")]
        public string JoinedDate { get; set; }
    }

    public class CandidateProfile
    {
        [BsonElement("resumeUrl")]
        public string ResumeUrl { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("yearsOfExperience")]
        public double YearsOfExperience { get; set; }

        [BsonElement("currentPosition")]
        public string CurrentPosition { get; set; }

        [BsonElement("currentCompany")]
        public string CurrentCompany { get; set; }

        [BsonElement("skills")]
        public List<string> Skills { get; set; }

        [BsonElement("languages")]
        public List<Language> Languages { get; set; }

        [BsonElement("education")]
        public List<Education> Education { get; set; }

        [BsonElement("certifications")]
        public List<Certification> Certifications { get; set; }

        [BsonElement("workExperience")]
        public List<WorkExperience> WorkExperience { get; set; }

        [BsonElement("projects")]
        public List<Project> Projects { get; set; }

        [BsonElement("expectedSalary")]
        public ExpectedSalary ExpectedSalary { get; set; }

        [BsonElement("jobPreferences")]
        public JobPreferences JobPreferences { get; set; }

        [BsonElement("availability")]
        public string Availability { get; set; }

        [BsonElement("willingToRelocate")]
        public bool WillingToRelocate { get; set; }

        [BsonElement("profileCompleteness")]
        public int ProfileCompleteness { get; set; }
    }

    public class Language
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("level")]
        public string Level { get; set; }
    }

    public class Education
    {
        [BsonElement("degree")]
        public string Degree { get; set; }

        [BsonElement("major")]
        public string Major { get; set; }

        [BsonElement("school")]
        public string School { get; set; }

        [BsonElement("startDate")]
        public string StartDate { get; set; }

        [BsonElement("endDate")]
        public string EndDate { get; set; }

        [BsonElement("graduationYear")]
        public int GraduationYear { get; set; }

        [BsonElement("gpa")]
        public double Gpa { get; set; }
    }

    public class Certification
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("issuer")]
        public string Issuer { get; set; }

        [BsonElement("issueDate")]
        public string IssueDate { get; set; }

        [BsonElement("expiryDate")]
        public string ExpiryDate { get; set; }

        [BsonElement("credentialId")]
        public string CredentialId { get; set; }
    }

    public class WorkExperience
    {
        [BsonElement("company")]
        public string Company { get; set; }

        [BsonElement("position")]
        public string Position { get; set; }

        [BsonElement("startDate")]
        public string StartDate { get; set; }

        [BsonElement("endDate")]
        public string EndDate { get; set; }

        [BsonElement("isCurrent")]
        public bool IsCurrent { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }
    }

    public class Project
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("role")]
        public string Role { get; set; }

        [BsonElement("startDate")]
        public string StartDate { get; set; }

        [BsonElement("endDate")]
        public string EndDate { get; set; }
    }

    public class ExpectedSalary
    {
        [BsonElement("min")]
        public decimal Min { get; set; }

        [BsonElement("max")]
        public decimal Max { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; }

        [BsonElement("isNegotiable")]
        public bool IsNegotiable { get; set; }
    }

    public class JobPreferences
    {
        [BsonElement("locations")]
        public List<string> Locations { get; set; }

        [BsonElement("workModes")]
        public List<string> WorkModes { get; set; }

        [BsonElement("categories")]
        public List<string> Categories { get; set; }

        [BsonElement("jobLevels")]
        public List<string> JobLevels { get; set; }

        [BsonElement("industries")]
        public List<string> Industries { get; set; }
    }

    public class UserStatistics
    {
        [BsonElement("totalApplications")]
        public int TotalApplications { get; set; }

        [BsonElement("pendingApplications")]
        public int PendingApplications { get; set; }

        [BsonElement("interviewingApplications")]
        public int InterviewingApplications { get; set; }

        [BsonElement("rejectedApplications")]
        public int RejectedApplications { get; set; }

        [BsonElement("profileViews")]
        public int ProfileViews { get; set; }

        [BsonElement("savedJobs")]
        public int SavedJobs { get; set; }
    }
}
