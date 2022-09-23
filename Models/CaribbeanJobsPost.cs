namespace trinibytes.Models;

using Microsoft.EntityFrameworkCore;

[Index(nameof(CaribbeanJobsJobId), IsUnique = true)]
public class CaribbeanJobsPost
{
    public int Id { get; set; }
    public string URL { get; set; }
    public int CaribbeanJobsJobId { get; set; }
    public string JobTitle { get; set; }
    public string? JobCompany { get; set; }
    public string? JobCategory { get; set; }
    public string? JobLocation { get; set; }
    public int? JobSalary { get; set; }
    public string? JobMinEducationRequirement { get; set; }
    public string FullJobDescription { get; set; }
    public string? JobEmploymentType { get; set; }
    public DateTime JobListingDate { get; set; }
    
    public DateTime LastModifiedDate { get; set; }
    
    public DateTime JobDeListingDate { get; set; }
    public bool JobListingIsActive { get; set; }

    public CaribbeanJobsPost()
    {
    }


    public CaribbeanJobsPost(string url, int caribbeanJobsJobId, string jobTitle, string jobCompany, string jobCategory,
        string jobLocation, int? jobSalary, string jobMinEducationRequirement, string fullJobDescription,
        DateTime jobListingDate)
    {
        URL = url;
        CaribbeanJobsJobId = caribbeanJobsJobId;
        JobTitle = jobTitle;
        JobCompany = jobCompany;
        JobCategory = jobCategory;
        JobLocation = jobLocation;
        JobSalary = jobSalary;
        JobMinEducationRequirement = jobMinEducationRequirement;
        FullJobDescription = fullJobDescription;
        JobListingDate = jobListingDate;
        JobListingIsActive = true;
    }
}