using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Company
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("slug")]
        public string Slug { get; set; }

        [BsonElement("website")]
        public string Website { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; }

        [BsonElement("employeeSize")]
        public string EmployeeSize { get; set; }

        [BsonElement("businessField")]
        public string BusinessField { get; set; }

        [BsonElement("taxCode")]
        public string TaxCode { get; set; }

        [BsonElement("foundedYear")]
        public int FoundedYear { get; set; }

        [BsonElement("introduction")]
        public string Introduction { get; set; }

        [BsonElement("vision")]
        public string Vision { get; set; }

        [BsonElement("mission")]
        public string Mission { get; set; }

        [BsonElement("coreValues")]
        public List<string> CoreValues { get; set; }

        [BsonElement("location")]
        public CompanyLocation Location { get; set; }

        [BsonElement("branches")]
        public List<Branch> Branches { get; set; }

        [BsonElement("tier")]
        public string Tier { get; set; }

        [BsonElement("tierExpireAt")]
        public DateTime? TierExpireAt { get; set; }

        [BsonElement("logoUrl")]
        public string LogoUrl { get; set; }

        [BsonElement("coverUrl")]
        public string CoverUrl { get; set; }

        [BsonElement("images")]
        public List<string> Images { get; set; }

        [BsonElement("benefits")]
        public List<string> Benefits { get; set; }

        [BsonElement("workingHours")]
        public WorkingHours WorkingHours { get; set; }

        [BsonElement("socialMedia")]
        public CompanySocialMedia SocialMedia { get; set; }

        [BsonElement("verified")]
        public bool Verified { get; set; }

        [BsonElement("verifiedAt")]
        public DateTime? VerifiedAt { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("totalJobs")]
        public int TotalJobs { get; set; }

        [BsonElement("activeJobs")]
        public int ActiveJobs { get; set; }

        [BsonElement("followers")]
        public int Followers { get; set; }

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
    }

    public class CompanyLocation
    {
        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("city")]
        public string City { get; set; }

        [BsonElement("district")]
        public string District { get; set; }

        [BsonElement("country")]
        public string Country { get; set; } = "Việt Nam";

        [BsonElement("lat")]
        public double Lat { get; set; }

        [BsonElement("lng")]
        public double Lng { get; set; }
    }

    public class Branch
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; }
    }

    public class WorkingHours
    {
        [BsonElement("monday")]
        public string Monday { get; set; }

        [BsonElement("tuesday")]
        public string Tuesday { get; set; }

        [BsonElement("wednesday")]
        public string Wednesday { get; set; }

        [BsonElement("thursday")]
        public string Thursday { get; set; }

        [BsonElement("friday")]
        public string Friday { get; set; }

        [BsonElement("saturday")]
        public string Saturday { get; set; }

        [BsonElement("sunday")]
        public string Sunday { get; set; }
    }

    public class CompanySocialMedia
    {
        [BsonElement("facebook")]
        public string Facebook { get; set; }

        [BsonElement("linkedin")]
        public string LinkedIn { get; set; }

        [BsonElement("youtube")]
        public string YouTube { get; set; }
    }
}
