using Microsoft.EntityFrameworkCore;
using Serilog;
using trinibytes;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<trinibytes.Models.MyDbContext>(
    options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("ConnStr")!, sqlServerOptionsAction:
            sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
    });
builder.Services.AddHostedService<BackgroundTasks.ScrapeCaribbeanJobsTask>();
builder.Services.AddSingleton<BackgroundTasks.ICustomServiceStopper, BackgroundTasks.ScrapeCaribbeanJobsTask>();
builder.Services.AddSingleton<BackgroundTasks.ICustomServiceStarter, BackgroundTasks.ScrapeCaribbeanJobsTask>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy => { policy.AllowAnyOrigin(); });
});
var app = builder.Build();

// set up logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .WriteTo.File("logs/trinibytes.log", rollingInterval: RollingInterval.Day, shared: true)
    .CreateLogger();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");
;

app.Run();