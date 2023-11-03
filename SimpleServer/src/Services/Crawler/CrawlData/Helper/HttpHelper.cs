namespace CrawlData.Helper
{
    public static class HttpHelper
    {
        public static Task<string> GetAsync(string url)
        {
            var httpClient = new HttpClient();
            return httpClient.GetStringAsync(url);
        }
    }
}