using System.Collections.Specialized;
using CrawlData.Helper;
using CrawlData.Infrastructor;
using CrawlData.Job;
using CrawlData.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    // Specify which file to write logs to and when need to create a new log file
    .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<DatabaseSettings>(config =>
        {
            config.ConnectionString = hostContext.Configuration
                .GetSection("DatabaseSettings:ConnectionString").Value
                ?? throw new("ConnectionString is null");

            config.DatabaseName = hostContext.Configuration
                .GetSection("DatabaseSettings:DatabaseName").Value
                ?? throw new("DatabaseName is null");

            config.CollectionName = hostContext.Configuration
                .GetSection("DatabaseSettings:CollectionName").Value
                ?? throw new("CollectionName is null");
        });
        services.AddTransient<MongoHelper>();
        services.AddScoped<IMongoCrawlerDBContext, MongoCrawlerDBContext>();
        services.AddScoped<CrawlJob>();
        services.AddScoped<ICrawlerService, CrawlerService>();
    }).UseConsoleLifetime();

var host = builder.Build();

MongoHelper database = host.Services.GetRequiredService<MongoHelper>();

var serviceProvider = host.Services.CreateScope().ServiceProvider;
await ScheduleJob(serviceProvider);
Console.ReadLine();

async Task ScheduleJob(IServiceProvider serviceProvider)
{
    // Use the binary serializer for Quartz.NET
    var props = new NameValueCollection
    {
        {"quartz.serializer.type", "binary"}
    };

    var factory = new StdSchedulerFactory(props);
    var scheduler = await factory.GetScheduler();
    scheduler.JobFactory = new CrawlJobFactory(serviceProvider);

    await  scheduler.Start();
    var job = JobBuilder.Create<CrawlJob>()
        .WithIdentity("CrawlJob", "CrawlGroup")
        .Build();
    var trigger = TriggerBuilder.Create()
        .WithIdentity("CrawlTrigger", "CrawlGroup")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(10)
            .RepeatForever())
        .Build();
    await scheduler.ScheduleJob(job, trigger);
}   