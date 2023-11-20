using System.Collections.Specialized;
using CrawlData.Helper;
using CrawlData.Infrastructor;
using CrawlData.Job;
using CrawlData.Service;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Serilog;
using AutoMapper;

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
        services.AddAutoMapper(typeof(Program));
        // MassTransit-RabbitMQ Configuration
        services.AddMassTransit(config =>
        {
            config.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(hostContext.Configuration["EventBusSettings:HostAddress"]);
                // cfg.UseHealthCheck(ctx);
            });
        });
    }).UseConsoleLifetime();

var host = builder.Build();

MongoHelper database = host.Services.GetRequiredService<MongoHelper>();

var serviceProvider = host.Services.CreateScope().ServiceProvider;
await ScheduleJob(serviceProvider);
Console.ReadLine();

static async Task ScheduleJob(IServiceProvider serviceProvider)
{
    // Use the binary serializer for Quartz.NET
    var props = new NameValueCollection
    {
        {"quartz.serializer.type", "binary"}
    };

    var factory = new StdSchedulerFactory(props);
    var scheduler = await factory.GetScheduler();
    scheduler.JobFactory = new CrawlJobFactory(serviceProvider);

    await scheduler.Start();
    var job = JobBuilder.Create<CrawlJob>()
        .WithIdentity("CrawlJob", "CrawlGroup")
        .Build();
    // Schedule the job to run every day at 2:00 AM
    var trigger = TriggerBuilder.Create()
        .WithIdentity("CrawlTrigger", "CrawlGroup")
        .StartNow()
        .WithDailyTimeIntervalSchedule(x => x
            .OnEveryDay()
            // set timezone in VietNam
            .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
            .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(2, 0)) // Set the desired time
        )
        .Build();

    await scheduler.ScheduleJob(job, trigger); 
}