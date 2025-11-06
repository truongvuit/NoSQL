using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Models
{
    public class Job
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("companyId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CompanyId { get; set; }

        [BsonElement("companySnapshot")]
        public CompanySnapshot CompanySnapshot { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("salary")]
        public Salary Salary { get; set; }

        [BsonElement("experience")]
        public string Experience { get; set; }

        [BsonElement("education")]
        public string Education { get; set; }

        [BsonElement("employmentType")]
        public string EmploymentType { get; set; }

        [BsonElement("gender")]
        public string Gender { get; set; }

        [BsonElement("ageRange")]
        public AgeRange AgeRange { get; set; }

        [BsonElement("startDate")]
        public string StartDate { get; set; }

        [BsonElement("endDate")]
        public string EndDate { get; set; }

        [BsonElement("vacancies")]
        public int Vacancies { get; set; }

        [BsonElement("workMode")]
        public string WorkMode { get; set; }

        [BsonElement("skills")]
        public List<string> Skills { get; set; }

        [BsonElement("categories")]
        public List<string> Categories { get; set; }

        [BsonElement("keywords")]
        public List<string> Keywords { get; set; }

        [BsonElement("jobDetails")]
        public string JobDetails { get; set; }

        [BsonElement("requirements")]
        public string Requirements { get; set; }

        [BsonElement("benefits")]
        public string Benefits { get; set; }

        [BsonElement("workplace")]
        public Workplace Workplace { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "draft";

        [BsonElement("priority")]
        public string Priority { get; set; } = "normal";

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("views")]
        public int Views { get; set; }

        [BsonElement("applicationCount")]
        public int ApplicationCount { get; set; }

        [BsonElement("rejectedCount")]
        public int RejectedCount { get; set; }

        [BsonElement("interviewedCount")]
        public int InterviewedCount { get; set; }

        [BsonElement("hiredCount")]
        public int HiredCount { get; set; }

        [BsonElement("lastActivityAt")]
        public string LastActivityAt { get; set; }

        [BsonElement("createdBy")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("deletedAt")]
        public DateTime? DeletedAt { get; set; }

        [BsonElement("schemaVersion")]
        public string SchemaVersion { get; set; } = "1.0";

        [BsonElement("applicants")]
        public List<Applicant> Applicants { get; set; } = new List<Applicant>();
    }

    public class CompanySnapshot
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("logoUrl")]
        public string LogoUrl { get; set; }

        [BsonElement("tier")]
        public string Tier { get; set; }
    }

    public class Salary
    {
        [BsonElement("min")]
        public decimal Min { get; set; }

        [BsonElement("max")]
        public decimal Max { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }
    }

    public class AgeRange
    {
        [BsonElement("min")]
        public int Min { get; set; }

        [BsonElement("max")]
        public int Max { get; set; }
    }

    public class Workplace
    {
        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("city")]
        public string City { get; set; }

        [BsonElement("district")]
        public string District { get; set; }
    }

    public class Applicant
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("jobId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string JobId { get; set; }

        [BsonElement("applicantId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ApplicantId { get; set; }

        [BsonElement("applicantSnapshot")]
        public ApplicantSnapshot ApplicantSnapshot { get; set; }

        [BsonElement("coverLetter")]
        public string CoverLetter { get; set; }

        [BsonElement("attachments")]
        public List<Attachment> Attachments { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Pending";

        [BsonElement("statusHistory")]
        public List<StatusHistory> StatusHistory { get; set; }

        [BsonElement("screening")]
        public Screening Screening { get; set; }

        [BsonElement("interviews")]
        public List<Interview> Interviews { get; set; }

        [BsonElement("appliedAt")]
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("viewedByRecruiterAt")]
        public DateTime? ViewedByRecruiterAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ApplicantSnapshot
    {
        [BsonElement("fullName")]
        public string FullName { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; }

        [BsonElement("avatar")]
        public string Avatar { get; set; }

        [BsonElement("resumeUrl")]
        public string ResumeUrl { get; set; }

        [BsonElement("currentPosition")]
        public string CurrentPosition { get; set; }

        [BsonElement("yearsOfExperience")]
        public double YearsOfExperience { get; set; }
    }

    public class Attachment
    {
        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("url")]
        public string Url { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("size")]
        public long Size { get; set; }
    }

    public class StatusHistory
    {
        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("changedAt")]
        public DateTime ChangedAt { get; set; }

        [BsonElement("changedBy")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ChangedBy { get; set; }

        [BsonElement("note")]
        public string Note { get; set; }
    }

    public class Screening
    {
        [BsonElement("score")]
        public int Score { get; set; }

        [BsonElement("matchPercentage")]
        public int MatchPercentage { get; set; }

        [BsonElement("strengths")]
        public List<string> Strengths { get; set; }

        [BsonElement("weaknesses")]
        public List<string> Weaknesses { get; set; }
    }

    public class Interview
    {
        [BsonElement("round")]
        public int Round { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("scheduledAt")]
        public DateTime ScheduledAt { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }

        [BsonElement("interviewer")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Interviewer { get; set; }

        [BsonElement("interviewerName")]
        public string InterviewerName { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("feedback")]
        public string Feedback { get; set; }

        [BsonElement("score")]
        public int Score { get; set; }

        [BsonElement("note")]
        public string Note { get; set; }
    }
}
