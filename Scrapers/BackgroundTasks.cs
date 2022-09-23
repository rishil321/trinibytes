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
            var scraper = new ScrapeCaribbeanJobsPosts();
            var jobsPosts = scraper.ScrapeCurrentCaribbeanJobsDataForTrinidad();
            jobsPosts = scraper.ScrapeFullJobPostDataFromCaribbeanJobs(jobsPosts);
            var result = scraper.UpsertScrapedJobPostsIntoDatabase(jobsPosts, _dbContext);
            await result;
            if (result.Result)
            {
                Log.Information("Finished fetching current posts successfully.");
            }
            else
            {
                Log.Information("Something went wrong while fetching current job posts.");
            }

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