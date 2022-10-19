using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using trinibytes.Models;

namespace trinibytes.Scrapers;

public class ScrapeCaribbeanJobsPosts
{
    private readonly IWebDriver? _driver;

    public ScrapeCaribbeanJobsPosts()
    {
        var options = new ChromeOptions();
        options.AddArgument("disable-dev-shm-usage"); // overcome limited resource problems
        options.AddArgument("no-sandbox"); // Bypass OS security model
        options.AddArgument("headless");
        options.AddArgument("enable-automation");
        var service = ChromeDriverService.CreateDefaultService("/usr/bin");
        service.LogPath = "/var/log/chromedriver.log";
        service.EnableVerboseLogging = true;
        _driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(20));
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
        //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
        _driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(20);
    }

    ~ScrapeCaribbeanJobsPosts()
    {
        _driver!.Quit();
    }

    public List<CaribbeanJobsPost> ScrapeCurrentCaribbeanJobsDataForTrinidad()
    {
        var scrapedJobPosts = new List<CaribbeanJobsPost>();
        try
        {
            _driver!.Url =
                "https://www.caribbeanjobs.com/ShowResults.aspx?Keywords=&autosuggestEndpoint=%2fautosuggest&Location=124&Category=&Recruiter=Company%2cAgency&btnSubmit=Search&=&PerPage=10000";
            Thread.Sleep(5000);
            IReadOnlyList<IWebElement> jobModules = _driver.FindElements(By.ClassName("module-content"));
            foreach (var jobModule in jobModules)
            {
                var titleElement = jobModule.FindElements(By.XPath(".//h2[@itemprop='title']/a"));
                if (titleElement.Count <= 0) continue;
                var jobTitle = titleElement.First().Text;
                var jobID = titleElement.First().GetAttribute("jobid");
                var jobURL = titleElement.First().GetAttribute("href");
                var salary = jobModule.FindElement(By.XPath(".//li[@itemprop='baseSalary']")).Text;
                var lastUpdated = jobModule.FindElement(By.XPath(".//li[@itemprop='datePosted']")).Text;
                var jobLocations = jobModule.FindElements(By.XPath(".//li[@itemprop='jobLocation']"));
                var jobLocation = "";
                foreach (var locationElement in jobLocations)
                {
                    jobLocation += locationElement.Text;
                }

                var jobDescription = jobModule.FindElement(By.XPath(".//p[@itemprop='description']/span")).Text;
                var scrapedJobPost = new CaribbeanJobsPost();
                scrapedJobPost.JobTitle = jobTitle;
                scrapedJobPost.URL = jobURL;
                try
                {
                    scrapedJobPost.JobSalary = int.Parse(salary);
                }
                catch
                {
                    scrapedJobPost.JobSalary = -1;
                }

                try
                {
                    scrapedJobPost.CaribbeanJobsJobId = int.Parse(jobID);
                }
                catch
                {
                    scrapedJobPost.CaribbeanJobsJobId = -1;
                }

                scrapedJobPosts.Add(scrapedJobPost);
            }
        }
        catch (Exception e)
        {
            Log.Error("\nException Caught!");
            Log.Error($"Message :{e.Message} ");
            Log.Error(e.ToString());
        }

        return scrapedJobPosts;
    }

    public List<CaribbeanJobsPost> ScrapeFullJobPostDataFromCaribbeanJobs(
        List<CaribbeanJobsPost> caribbeanJobPostData)
    {
        try
        {
            foreach (var jobPostData in caribbeanJobPostData)
            {
                // load the URL for the job
                _driver!.Url = jobPostData.URL;
                // parse the data that we want to store for the job
                var jobPostTitle = _driver.FindElement(By.XPath(".//h1[@class='job-details--title']")).Text;
                var jobPostCompany =
                    _driver.FindElement(By.XPath(".//h2[@class='job-details--company']")).Text;
                var jobPostJobId = _driver.FindElement(By.XPath(".//input[@id='JobId']"))
                    .GetAttribute("value");
                var jobPostLocation = _driver.FindElement(By.XPath(".//li[@class='location']")).Text;
                var jobPostSalary = _driver.FindElement(By.XPath(".//li[@class='salary']")).Text;
                var jobPostEmploymentType =
                    _driver.FindElement(By.XPath(".//li[@class='employment-type']")).Text;
                var jobPostFullTextNode = _driver.FindElement(By.XPath(".//div[@class='job-details']"));
                // build the full job description
                var jobPostFullText = "";
                var unwantedText = new List<string> {"\r\n", ""};
                foreach (var textNode in jobPostFullTextNode.Text)
                {
                    if (!unwantedText.Any(w => textNode.Equals(w)))
                    {
                        jobPostFullText += textNode;
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
        }
        catch (Exception exc)
        {
            Log.Error(exc.ToString());
        }

        return caribbeanJobPostData;
    }
    
    ///<summary>
    /// This method takes a dictionary of scraped job posts from Caribbeanjobs.com
    /// then creates objects for the model using EF core and finally inserts/updates
    /// the database using these objects.
    /// The method returns true if the entire process is successful, else it returns false. 
    /// </summary>
    public async Task<bool> UpsertScrapedJobPostsIntoDatabase(List<CaribbeanJobsPost> scrapedJobPosts,
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
                        scrapedJobPost.LastModifiedDate = DateTime.Today;
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