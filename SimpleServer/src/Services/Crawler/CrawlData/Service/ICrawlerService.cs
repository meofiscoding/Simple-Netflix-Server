using System;

namespace CrawlData.Service
{
    public interface ICrawlerService
    {
        Task CrawlMovieDataAsync();

        Task TestCronJob();
    }
}
