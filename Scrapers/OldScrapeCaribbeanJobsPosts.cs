using trinibytes.Models;

namespace trinibytes.Scrapers;
using System.Collections.Specialized;
using CsvHelper;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System.Globalization;
using System.Runtime.CompilerServices;
using Serilog;

public static class OldScrapeCaribbeanJobsPosts
{
    /// <summary>
    /// Fetch the URLs of all active job posts for Trinidad and Tobago
    /// from caribbeanjobs.com
    /// </summary>
    /// <returns></returns>
    public static List<CaribbeanJobsPost> ScrapeCurrentTrinidadCaribbeanJobsPosts()
    {
        try
        {
            // create list of objects to store all the model objects
            var scrapedJobPostData = new List<CaribbeanJobsPost>();
            // set up page number and set jobs per page to iterate through all pages of job listings
            var currentPageNum = 1;
            const int jobsPerPage = 100;
            // create the URL to fetch jobs from
            var url =
                $"https://www.caribbeanjobs.com/ShowResults.aspx?Keywords=&autosuggestEndpoint=%2fautosuggest&Location=124&Category=&Recruiter=Company%2cAgency&btnSubmit=Search&PerPage={jobsPerPage}&Page={currentPageNum}";
            // set up the web scraping agent
            var webAgent = new HtmlWeb
            {
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.85 Safari/537.36 Edg/90.0.818.46"
            };

            // load the URL with the first page of jobs
            var doc = webAgent.Load(url);
            // calculate the number of jobs currently listed
            var totalJobsLabel = doc.DocumentNode.SelectSingleNode(".//div[@class='sort-by-wrap']/label[1]");
            var totalJobs = decimal.Parse(totalJobsLabel.InnerText.Split(": ")[1]);
            // determine how many pages we will need to iterate over
            var totalPagesToFetch = Math.Ceiling(totalJobs / jobsPerPage);
            // set up a loop to scrape all jobs from the site
            while (currentPageNum <= totalPagesToFetch)
            {
                // if this is a new page to be loaded/fetched
                if (currentPageNum != 1)
                {
                    doc = webAgent.Load(url);
                    Log.Debug($"Now fetching page {currentPageNum} of {totalPagesToFetch}.");
                }

                // get all the job posts on the current page
                var allJobPostsOnPage = doc.DocumentNode.SelectNodes(".//div[@class='job-result-title']");
                // only store the URL to the full job post for now
                foreach (var jobPost in allJobPostsOnPage)
                {
                    // get the URL string from the post
                    var jobPostUrl = jobPost.SelectSingleNode(".//h2/a").Attributes[1].Value;
                    Log.Debug($"Now adding {jobPostUrl} to list.");
                    //store the URL in a new model object for the database
                    var scrapedJobPost = new CaribbeanJobsPost();
                    scrapedJobPost.URL = jobPostUrl;
                    scrapedJobPostData.Add(scrapedJobPost);
                }

                // now increment the page number and fetch again
                currentPageNum++;
                url =
                    $"https://www.caribbeanjobs.com/ShowResults.aspx?Keywords=&autosuggestEndpoint=%2fautosuggest&Location=124&Category=&Recruiter=Company%2cAgency&btnSubmit=Search&PerPage={jobsPerPage}&Page={currentPageNum}";
            }

            Log.Debug("All job URLs fetched successfully.");
            return scrapedJobPostData;
        }
        catch (Exception exc)
        {
            Log.Error(exc.ToString());
            return null;
        }
    }


    ///<summary>
    /// This method navigates to each CaribbeanJobs.com URL fetched and gets the full
    /// data for each job post
    /// </summary>
    public static List<CaribbeanJobsPost> ScrapeFullJobPostDataFromCaribbeanJobs(
        List<CaribbeanJobsPost> caribbeanJobPostData)
    {
        try
        {
            // set up the web scraping agent
            var webAgent = new HtmlWeb
            {
                UserAgent =
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11"
            };
            foreach (var jobPostData in caribbeanJobPostData)
            {
                //construct the full URL for the caribbean jobs post
                var fullUrl = "https://www.caribbeanjobs.com" + jobPostData.URL;
                // load the URL for the job
                var doc = webAgent.Load(fullUrl);
                // parse the data that we want to store for the job
                var jobPostTitle = doc.DocumentNode.SelectSingleNode(".//h1[@class='job-details--title']").InnerText;
                var jobPostCompany =
                    doc.DocumentNode.SelectSingleNode(".//h2[@class='job-details--company']").InnerText;
                var jobPostJobId = doc.DocumentNode.SelectSingleNode(".//input[@id='JobId']")
                    .GetAttributeValue("value");
                var jobPostLocation = doc.DocumentNode.SelectSingleNode(".//li[@class='location']").InnerText;
                var jobPostSalary = doc.DocumentNode.SelectSingleNode(".//li[@class='salary']").InnerText;
                var jobPostEmploymentType =
                    doc.DocumentNode.SelectSingleNode(".//li[@class='employment-type']").InnerText;
                var jobPostFullTextNode = doc.DocumentNode.SelectSingleNode(".//div[@class='job-details']");
                // build the full job description
                var jobPostFullText = "";
                var unwantedText = new List<string> {"\r\n", ""};
                foreach (var textNode in jobPostFullTextNode.SelectNodes(".//text()"))
                {
                    if (!unwantedText.Any(w => textNode.InnerText.Equals(w)))
                    {
                        jobPostFullText += textNode.InnerText.Trim();
                    }
                }

                // store the parsed data in the model object
                jobPostData.JobTitle = jobPostTitle;
                jobPostData.JobCompany = jobPostCompany;
                jobPostData.JobLocation = jobPostLocation;
                try
                {
                    jobPostData.JobSalary = int.Parse(jobPostSalary);
                }
                catch
                {
                    jobPostData.JobSalary = -1;
                }

                try
                {
                    jobPostData.CaribbeanJobsJobId = int.Parse(jobPostJobId);
                }
                catch
                {
                    jobPostData.CaribbeanJobsJobId = -1;
                }

                jobPostData.JobEmploymentType = jobPostEmploymentType;
                jobPostData.FullJobDescription = jobPostFullText;

                Log.Debug($"Successfully parsed data for {jobPostTitle} from {jobPostCompany}.");
            }

            Log.Debug("All job post data added to list successfully.");
            return caribbeanJobPostData;
        }
        catch (Exception exc)
        {
            Log.Error(exc.ToString());
            return null;
        }
    }

    ///<summary>
    /// This method takes a dictionary of scraped job posts from Caribbeanjobs.com
    /// then creates objects for the model using EF core and finally inserts/updates
    /// the database using these objects.
    /// The method returns true if the entire process is successful, else it returns false. 
    /// </summary>
    public static async Task<bool> UpsertScrapedJobPostsIntoDatabase(List<CaribbeanJobsPost> scrapedJobPosts,
        MyDbContext _dbContext)
    {
        try
        {
            foreach (var scrapedJobPost in scrapedJobPosts)
            {
                try
                {
                    if (_dbContext.CaribbeanJobsPosts
                        .Any(obj => obj.CaribbeanJobsJobId == scrapedJobPost.CaribbeanJobsJobId))
                    {
                        //this object exists in the database already. we need to update
                        var existingJobPost = _dbContext.CaribbeanJobsPosts
                            .Single(obj => obj.CaribbeanJobsJobId == scrapedJobPost.CaribbeanJobsJobId);
                        existingJobPost.FullJobDescription = scrapedJobPost.FullJobDescription;
                        existingJobPost.LastModifiedDate = DateTime.Today;
                    }
                    else
                    {
                        //this is a new object to create
                        scrapedJobPost.JobListingDate = DateTime.Today;
                        _dbContext.CaribbeanJobsPosts.Add(scrapedJobPost);
                    }

                    Log.Information(
                        $"Updated/inserted {scrapedJobPost.JobTitle} from {scrapedJobPost.JobCompany} into database.");
                }
                catch (Exception exc)
                {
                    Log.Error(exc.ToString());
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception exc)
        {
            Log.Error(exc.ToString());
            return false;
        }
    }
}