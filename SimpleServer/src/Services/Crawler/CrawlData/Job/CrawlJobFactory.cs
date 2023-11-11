using System;
using AutoMapper;
using Quartz;
using Quartz.Spi;

namespace CrawlData.Job
{
    public class CrawlJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CrawlJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return (IJob)_serviceProvider.GetService(typeof(CrawlJob));
        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}
