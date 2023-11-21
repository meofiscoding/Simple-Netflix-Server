using System;
using AutoMapper;
using CrawlData.Service;
using Microsoft.Extensions.Options;
using Quartz;

namespace CrawlData.Job
{
    public class CrawlJob : IJob
    {
        private readonly ICrawlerService _crawlerService;
        public CrawlJob(ICrawlerService crawlerService)
        {
            _crawlerService = crawlerService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await _crawlerService.CrawlMovieDataAsync();
            // _crawlerService.TestCronJob();
            // return Task.CompletedTask;
            
            //retrieve all triggers for the job
            var triggers = await context.Scheduler.GetTriggersOfJob(context.JobDetail.Key);
            // compare them to the one executing and if they were scheduled after it, unschedule the job (triggers will be deleted).
            foreach (ITrigger trigger in triggers)
            {
                if (trigger.GetPreviousFireTimeUtc() > context.FireTimeUtc)
                {
                    await context.Scheduler.UnscheduleJob(trigger.Key);
                }
            }

        }
    }
}
