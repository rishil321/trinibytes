using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using trinibytes.Models;
using trinibytes.Scrapers;
using Microsoft.EntityFrameworkCore;

namespace trinibytes;

public static class BackgroundTasks
{
    public interface ICustomServiceStopper
    {
        Task StopAsync(CancellationToken token = default);
    }

    public interface ICustomServiceStarter
    {
        Task StartAsync(CancellationToken token = default);
    }

    public class ScrapeCaribbeanJobsTask : BackgroundService, ICustomServiceStopper, ICustomServiceStarter
    {
        private static Timer _caribbeanJobsScraperTimer;
        private static bool _isScraperEnabled;
        private static bool _isScraperRunning;
        private readonly MyDbContext _dbContext;
        private static IServiceScopeFactory _serviceScopeFactory;

        public ScrapeCaribbeanJobsTask(IServiceScopeFactory scopeFactory)
        {
            _serviceScopeFactory = scopeFactory;
        }

        public static bool getIsScraperEnabled()
        {
            return _isScraperEnabled;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Debug("Background job started.");
            // set up a timer to call the DoWork function 10 seconds after the program is started, then repeat every 1 day afterwards
            _caribbeanJobsScraperTimer = new Timer(RunCaribbeanJobsScraper, null, TimeSpan.FromSeconds(10),
                TimeSpan.FromHours(24));
            _isScraperEnabled = true;
        }

        private static async void RunCaribbeanJobsScraper(object? state)
        {
            if (_isScraperRunning) return;
            _isScraperRunning = true;
            using var scope = _serviceScopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            //fetch all the URLs for the currently active job posts
            var jobPosts = ScrapeCaribbeanJobsPosts.ScrapeCurrentTrinidadCaribbeanJobsPosts();
            // navigate to the full URL and fetch all the data that we require
            var fullJobPostData = ScrapeCaribbeanJobsPosts.ScrapeFullJobPostDataFromCaribbeanJobs(jobPosts);
            //add or update all of the job posts from the database
            var result = ScrapeCaribbeanJobsPosts.UpsertScrapedJobPostsIntoDatabase(fullJobPostData, _dbContext);
            await result;
            Log.Information("Finished fetching current posts.");
            _isScraperRunning = false;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _caribbeanJobsScraperTimer.Change(Timeout.Infinite, 0);
            _isScraperEnabled = false;
            Log.Debug("Background job stopped.");
        }

        Task ICustomServiceStopper.StopAsync(CancellationToken token = default)
        {
            return StopAsync(token);
        }

        Task ICustomServiceStarter.StartAsync(CancellationToken token = default)
        {
            return ExecuteAsync(token);
        }
    }
}