using System;
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
        public Task Execute(IJobExecutionContext context)
        {
            _crawlerService.CrawlMovieData();
            // _crawlerService.TestCronJob();
            return Task.CompletedTask;
        }
    }
}
