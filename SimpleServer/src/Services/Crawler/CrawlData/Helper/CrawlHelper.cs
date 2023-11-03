using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using CrawlData.Enum;
using CrawlData.Model;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CrawlData.Helper
{
    public static partial class CrawlHelper
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

            // Define the pattern to match article elements with class names starting with "item"
            string pattern = "^item\\s";

            // Get all movie item in article with class had this format "item <movie type>"
            var movieHTMLElements = document.DocumentNode.Descendants("article")
                .Where(x => Regex.IsMatch(x.GetAttributeValue("class", ""), pattern, RegexOptions.IgnoreCase))
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
                        MovieName = item.Descendants("h3").FirstOrDefault()?.InnerText,
                        UrlDetail = item.Descendants("a").FirstOrDefault()?.GetAttributeValue("href", ""),
                        Poster = item.Descendants("img").FirstOrDefault()?.GetAttributeValue("src", ""),
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

        private static async Task ExtractDataToJsonAsync(List<MovieItem> movieItem)
        {
            // crating the json output file
            var json = JsonSerializer.Serialize(movieItem, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            // write the data crawled to file
            await File.WriteAllTextAsync("phimmoiMovieCrawledData.json", json);

            // check if file exist
            if (File.Exists("phimmoiMovieCrawledData.json"))
            {
                Console.WriteLine($" Crawling data completed!\n {movieItem.Count} movies crawled!\n You can check output file at: {Directory.GetCurrentDirectory()}");
            }
            else
            {
                Console.WriteLine("Extract data to json failed!");
            }
        }

        public static async Task<MovieItem> CrawlMovieDetailAsync(MovieItem movie)
        {
            if (string.IsNullOrEmpty(movie.UrlDetail))
            {
                throw new Exception($"Movie url of movie {movie.MovieName} is null or empty");
            }

            // to open Chrome in headless mode 
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");

            // starting a Selenium instance 
            using (var driver = new ChromeDriver(chromeOptions))
            {
                driver.Navigate().GoToUrl(movie.UrlDetail);
                // wait until div with id playeroptions had no class "onload"
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(x => x.FindElement(By.Id("playeroptions")).GetAttribute("class") != "onload");
                // if timeout and div with id playeroptions still have class "onload" then throw exception
                if (driver.FindElement(By.Id("playeroptions")).GetAttribute("class") == "onload")
                {
                    throw new Exception($"Iframe of movie {movie.MovieName} is not loaded");
                }
                // get movie description from div which itemprop="description"
                string? description = driver.FindElement(By.CssSelector("div[itemprop='description']"))?.Text;
                movie.Description = HttpUtility.HtmlDecode(description);
                // get movie streaming url
                if (movie.MovieCategory == Category.Movies)
                {
                    // get the first iframe element
                    var iframe = driver.FindElement(By.TagName("iframe"));
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
                            movie.StreamingUrl = HttpUtility.UrlDecode(iframeSrc);
                        }
                        else
                        {
                            // call HTTP GET to get the streaming url
                            movie.StreamingUrl = await GetStreamingUrlAsync(iframeSrc);
                        }
                    }
                    else
                    {
                        var url = queryParams.AllKeys
                            .Where(x => queryParams[x].StartsWith("http"))
                            .Select(x => HttpUtility.UrlDecode(queryParams[x]))
                            .FirstOrDefault() ?? throw new Exception("No .m3u8 url found");

                        // Get the final url in query param until not contain query param
                        while (url.Contains('?'))
                        {
                            // get all query params if exist in url
                            queryParams = HttpUtility.ParseQueryString(url);
                            url = queryParams.AllKeys
                                .Where(x => queryParams[x].StartsWith("http"))
                                .Select(x => HttpUtility.UrlDecode(queryParams[x]))
                                .FirstOrDefault() ?? throw new Exception("No .m3u8 url found");
                        }

                        if (url.Contains(".m3u8"))
                        {
                            movie.StreamingUrl = url;
                        }
                        else
                        {
                            // call HTTP GET to get the streaming url
                            movie.StreamingUrl = await GetStreamingUrlAsync(url);
                        }
                    }
                }
                else
                {
                    // movie.StreamingUrl = document.DocumentNode.Descendants("iframe")
                    //     .FirstOrDefault(x => x.GetAttributeValue("id", "").Equals("iframe-embed"))?.GetAttributeValue("data-src", "");
                }
            }

            return movie;
        }

        private static async Task<string> GetStreamingUrlAsync(string url)
        {
            try
            {
                var response = await HttpHelper.GetAsync(url);
                // get the url contain .m3u8 in response, url must start with "http"
                var streamingUrl = StreamingUrlRegex().Match(response).Value;
                return HttpUtility.UrlDecode(streamingUrl);
            }
            catch
            {
                // throw exception url invalid, can not call HTTP GET
                throw new HttpRequestException($"Url {url} is invalid");
            }
        }

        [GeneratedRegex("http.*?\\.m3u8")]
        private static partial Regex StreamingUrlRegex();
    }
}
