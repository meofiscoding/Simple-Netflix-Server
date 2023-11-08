using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;
using CrawlData.Enum;
using CrawlData.Model;
using CrawlData.Utils;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace CrawlData.Helper
{
    public static class CrawlHelper
    {
        public static List<MovieItem> CrawlMovieInfo(string url)
        {
            HtmlDocument document = LoadDocument(url);

            var movieItem = new List<MovieItem>();

            /* Example of item:
             <article id="post-featured-50405" class="item tvshows">
                <div class="poster">
                  <img
                    src="https://phimmoiyyy.net/wp-content/uploads/2023/10/Loki.jpg"
                    alt="Loki: Phần 2"
                  />
                  <div class="trangthai">Tập 4 Vietsub</div>
                  <div class="featu">NỔI BẬT</div>
                  <a href="https://phimmoiyyy.net/phim-bo/loki-phan-2-154230"
                    ><div class="see play1"></div
                  ></a>
                </div>
                <div class="data dfeatur">
                  <h3>
                    <a href="https://phimmoiyyy.net/phim-bo/loki-phan-2-154230"
                      >Loki: Phần 2</a
                    >
                  </h3>
                  <span>2023</span>
                </div>
              </article>
*/

            // Get all movie item in article with class had this format "item <movie type>"
            var movieHTMLElements = document.DocumentNode.Descendants("article")
                .Where(x => CustomRegex.MovieItemRegex().IsMatch(x.GetAttributeValue("class", "")))
                .ToList();

            foreach (var item in movieHTMLElements)
            {
                var movie = movieItem.Find(x => x.UrlDetail == item.Descendants("a").FirstOrDefault()?.GetAttributeValue("href", "").Trim());
                var parent = item.ParentNode.GetAttributeValue("id", "");

                // check if movie already exist
                if (movie != null)
                {
                    MovieHelper.AssignMovieTag(movie, parent);
                }
                else
                {
                    // if movie not exist, then add it to list
                    movie = new MovieItem
                    {
                        MovieName = item.Descendants("h3").FirstOrDefault()?.InnerText ?? "",
                        UrlDetail = item.Descendants("a").FirstOrDefault()?.GetAttributeValue("href", ""),
                        Poster = item.Descendants("img").FirstOrDefault()?.GetAttributeValue("src", ""),
                        Status = item.Descendants("div").FirstOrDefault(x => x.GetAttributeValue("class", "").Contains("trangthai"))?.InnerText.Trim() ?? "",
                        // assign category for movie based on value of class="item <movie type>"
                        Tags = new List<Tag>(),
                        MovieCategory = System.Enum.TryParse<Category>(item.GetAttributeValue("class", "").Split(" ").LastOrDefault(), true, out var result) ? result : Category.Movies
                    };
                    // if parent div id contains "featured-titles" then add tag "Hot"
                    MovieHelper.AssignMovieTag(movie, parent);
                    movieItem.Add(movie);
                }
            }
            return movieItem;
        }

        private static HtmlDocument LoadDocument(string url)
        {
            var web = new HtmlWeb();
            // loading the target web page 
            return web.Load(url);
        }

        private static async Task<string> GetPlayListUrlOfMovie(string movieUrl)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless", "--disable-gpu","--no-sandbox", "--disable-dev-shm-usage");
            chromeOptions.PageLoadStrategy = PageLoadStrategy.None;
            //chromeOptions.PageLoadStrategy = PageLoadStrategy.Eager;
            var driver = new ChromeDriver(chromeOptions);
            // wait until page state is ready
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(2);
            bool isOnloadExist = true;
            var bodyHtml = "";
            try
            {
                // navigate to movie url
                driver.Navigate().GoToUrl(movieUrl);
                // wait until document state is ready
                // wait until  excute script that verify playeroptions div is exist
                // wait.Until(x => ((IJavaScriptExecutor)x).ExecuteScript("return document.getElementById('playeroptions')") != null);
                // wait.Until(x => ((IJavaScriptExecutor)x).ExecuteScript("return document.readyState").Equals("interactive"));
                bodyHtml = driver.FindElement(By.TagName("body")).GetAttribute("innerHTML");
                var playerOptions = driver.FindElement(By.Id("playeroptions"));
                // if timeout and div with id playeroptions still have class "onload" then throw exception
                while (playerOptions.GetAttribute("class").Contains("onload"))
                {
                    // reload page
                    driver.Navigate().Refresh();
                    // wait.Until(x => ((IJavaScriptExecutor)x).ExecuteScript("return document.getElementById('playeroptions')") != null);
                    // wait.Until(x => ((IJavaScriptExecutor)x).ExecuteScript("return document.readyState").Equals("interactive"));
                    // wait tuntil div with id playeroptions exist
                    playerOptions = driver.FindElement(By.Id("playeroptions"));
                }
                isOnloadExist = playerOptions.GetAttribute("class").Contains("onload");
                // get the first iframe element
                var iframe = driver.FindElements(By.TagName("iframe")).FirstOrDefault() ?? throw new Exception("No iframe element found");
                var iframeSrc = iframe.GetAttribute("src");
                // get all query params if exist in src attribute
                var queryParams = HttpUtility.ParseQueryString(iframeSrc);
                // Case 1: Not have query param: 
                // If iframe src contain .m3u8 then set streaming url = decode iframe src
                // Else call HTTP GET to get the streaming url
                // Case 2: Have query param:
                // Filter all query param to get value which value start with "http"
                // Decode url
                // If url contain query param which value contain .m3u8 then set streaming url = url
                // Else call HTTP GET to get the streaming url
                if (queryParams.Count == 0)
                {
                    if (iframeSrc.Contains(".m3u8"))
                    {

                        driver.Quit();
                        return HttpUtility.UrlDecode(iframeSrc);
                    }
                    else
                    {

                        driver.Quit();
                        // call HTTP GET to get the streaming url
                        return await GetStreamingUrlAsync(iframeSrc);
                    }
                }
                else
                {
                    var url = queryParams.AllKeys
                        .Where(x => queryParams[x]?.StartsWith("http") ?? false)
                        .Select(x => HttpUtility.UrlDecode(queryParams[x]))
                        .FirstOrDefault() ?? throw new NullReferenceException("No .m3u8 url found");

                    // Get the final url in query param until not contain query param
                    while (url.Contains('?'))
                    {
                        // get all query params if exist in url
                        queryParams = HttpUtility.ParseQueryString(url);
                        url = queryParams.AllKeys
                            .Where(x => queryParams[x]?.StartsWith("http") ?? false)
                            .Select(x => HttpUtility.UrlDecode(queryParams[x]))
                            .FirstOrDefault() ?? throw new NullReferenceException("No .m3u8 url found");
                    }

                    if (url.Contains(".m3u8"))
                    {
                        driver.Quit();
                        return url;
                    }
                    else
                    {
                        driver.Quit();
                        // call HTTP GET to get the streaming url
                        return await GetStreamingUrlAsync(url);
                    }
                }
            }
            catch (Exception ex)
            {
                // quit chrome driver instance
                driver.Quit();
                //throw new Exception($"Get playlist url of movie {movieUrl} got error: {ex.Message} when onLoad element is {isOnloadExist} with body html is {bodyHtml}");
                // TODO: Implement logging the error
                Log.Error($"Get playlist url of movie {movieUrl} got error: {ex.Message} when onLoad element is {isOnloadExist} with body html is {bodyHtml}");
                return "";
            }
        }

        public static async Task<MovieItem> CrawlMovieDetailAsync(MovieItem movie)
        {
            if (string.IsNullOrEmpty(movie.UrlDetail))
            {
                throw new Exception($"Movie url of movie {movie.MovieName} is null or empty");
            }

            // Load document from url
            HtmlDocument document = LoadDocument(movie.UrlDetail);

            // get movie description from div which itemprop="description"
            string description = document.DocumentNode.Descendants("div")
                .FirstOrDefault(x => x.GetAttributeValue("itemprop", "") == "description")?.InnerText.Trim() ?? "";

            movie.Description = HttpUtility.HtmlDecode(description);

            if (movie.MovieCategory == Category.Movies)
            {
                if (movie.StreamingUrls.Count == 0)
                {
                    string streamingUrl = await GetPlayListUrlOfMovie(movie.UrlDetail);
                    movie.StreamingUrls = new Dictionary<string, string> { { "0", streamingUrl } };
                }
            }
            else
            {
                // status format: "Tập <number> Vietsub" or "FULL <number>/ <number> RAW"
                // get number of available episode
                var availableEpisode = int.Parse(CustomRegex.EpisodesRegex().Match(movie.Status).Value);

                // get ul with class = episodios
                var ulEpisode = document.DocumentNode.Descendants("ul")
                    .FirstOrDefault(x => x.GetAttributeValue("class", "") == "episodios")
                    ?? throw new Exception($"No episode found for movie {movie.MovieName}");
                // get all episode number that not crawled
                var remainStreamingUrls = Enumerable
                    .Range(
                        // start from number of first li value in ulEpisode
                        int.Parse(CustomRegex.EpisodesRegex().Match(ulEpisode.Descendants("li").FirstOrDefault()?.InnerText ?? "").Value),
                        availableEpisode
                    )
                    .Where(x => string.IsNullOrEmpty(movie.StreamingUrls.GetValueOrDefault(x.ToString())))
                    .ToList();
                // get all episodeUrl which episode appear in remainStreamingUrls
                Dictionary<int, string?> episodeUrls = ulEpisode.Descendants("li")
                    .Where(x => remainStreamingUrls.Contains(int.Parse(CustomRegex.EpisodesRegex().Match(x.InnerText).Value)))
                    .ToDictionary(x => int.Parse(CustomRegex.EpisodesRegex().Match(x.InnerText).Value), x => x.Descendants("a").FirstOrDefault()?.GetAttributeValue("href", ""));

                var streamingUrlsDict = new ConcurrentDictionary<int, string>();

                var semaphore = new SemaphoreSlim(4); // Set the maximum degree of parallelism

                var tasks = episodeUrls.Select(async item =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var streamingUrl = await GetPlayListUrlOfMovie(item.Value ?? throw new Exception("No episode url found"));
                        streamingUrlsDict.TryAdd(item.Key, streamingUrl);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                // Re-order streaming url by episode number and add to movie.streamingUrls
                await Task.WhenAll(tasks);
                // preserve non null value of streamingUrls and update new value for all key in remainStreamingUrls
                foreach (var item in streamingUrlsDict)
                {
                    movie.StreamingUrls[item.Key.ToString()] = item.Value;
                }
            }
            return movie;
        }

        private static async Task<string> GetStreamingUrlAsync(string url)
        {
            try
            {
                var response = await HttpHelper.GetAsync(url);
                // get the url contain .m3u8 in response
                var streamingUrl = CustomRegex.StreamingUrlRegex().Match(response).Value;
                if (!streamingUrl.Contains("http"))
                {
                    // get base url from url https://1080.opstream4.com/share/006730204165965b7e5f7dc9c4b63f92 to https://1080.opstream4.com
                    var baseUrl = new Uri(url).GetLeftPart(UriPartial.Authority);
                    // concat base url with streaming url 
                    streamingUrl = baseUrl + streamingUrl;
                }
                return HttpUtility.UrlDecode(streamingUrl);
            }
            catch (Exception ex)
            {
                // throw exception url invalid, can not call HTTP GET
                Log.Error($"Url {url} got error: {ex.Message}");
                return "";
            }
        }
    }
}
