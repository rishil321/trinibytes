using trinibytes.Models;
using trinibytes.Scrapers;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Xml.Linq;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace trinibytes.Controllers
{
    [ApiController]
    [Route("CaribbeanJobsPosts")]
    public class CaribbeanJobsPostController : ControllerBase
    {
        readonly CultureInfo culture = new("en-US");
        private readonly MyDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly BackgroundTasks.ICustomServiceStopper _stopper;
        private readonly BackgroundTasks.ICustomServiceStarter _starter;

        public CaribbeanJobsPostController(MyDbContext context, IConfiguration configuration,
            BackgroundTasks.ICustomServiceStopper stopper, BackgroundTasks.ICustomServiceStarter starter)
        {
            _dbContext = context;
            _configuration = configuration;
            _stopper = stopper;
            _starter = starter;
        }

        [HttpPost]
        [Route("StopScrapeCaribbeanJobsPosts")]
        public async Task<bool> StopScrapeCaribbeanJobsPosts()
        {
            Log.Information("Cancellation of scraping CaribbeanJobs posts requested from UI.");
            try
            {
                await _stopper.StopAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return false;
            }
        }

        [HttpPost]
        [Route("StartScrapeCaribbeanJobsPosts")]
        public async Task<bool> StartScrapeCaribbeanJobsPosts()
        {
            Log.Information("Starting of scraping CaribbeanJobs posts requested from UI.");
            try
            {
                await _starter.StartAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return false;
            }
        }

        [HttpGet]
        [Route("GetLatestLogs")]
        public async Task<string> GetLatestLogs()
        {
            try
            {
                var lines = "";
                var latestLogFile = Directory.GetFiles("logs/")
                    .Select(x => new FileInfo(x))
                    .OrderByDescending(x => x.LastWriteTime)
                    .Take(1)
                    .ToArray().Last().ToString();
                //var lastLines = System.IO.File.ReadLines(latestLogFile).TakeLast(10);
                await using (var inStream = new FileStream(latestLogFile, FileMode.Open,
                                 FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(inStream))
                    {
                        while (sr.Peek() >= 0) // reading the old data
                        {
                            lines += await sr.ReadLineAsync();
                            lines += '\n';
                        }
                    }
                }

                return JsonSerializer.Serialize(lines);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return JsonSerializer.Serialize("Error in method");
            }
        }

        [HttpGet]
        [Route("CheckIsScraperRunning")]
        public async Task<bool> CheckIsScraperRunning()
        {
            try
            {
                return BackgroundTasks.ScrapeCaribbeanJobsTask.getIsScraperEnabled();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return false;
            }
        }
    }
}