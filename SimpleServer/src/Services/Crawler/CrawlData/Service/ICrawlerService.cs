using System;

namespace CrawlData.Service
{
    public interface ICrawlerService
    {
        Task CrawlMovieData();

        void TestCronJob();
    }
}
