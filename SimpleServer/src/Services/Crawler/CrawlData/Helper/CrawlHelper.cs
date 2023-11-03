using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using CrawlData.Enum;
using CrawlData.Model;
using HtmlAgilityPack;

namespace CrawlData.Helper
{
    public static partial class CrawlHelper
    {
        public static async Task<List<MovieItem>> CrawlMovieInfoAsync(string url)
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
            HtmlDocument document = LoadDocument(movie.UrlDetail);

            // get movie description from div which itemprop="description"
            string? description = document.DocumentNode.Descendants("div")
                .FirstOrDefault(x => x.GetAttributeValue("itemprop", "").Equals("description"))?.InnerText.Trim() ?? "";
            movie.Description = HttpUtility.HtmlDecode(description);
            // get movie streaming url
            if (movie.MovieCategory == Category.Movies)
            {
                // get the first iframe element
                var iframe = document.DocumentNode.Descendants("iframe").FirstOrDefault();
                var iframeSrc = iframe?.GetAttributeValue("src", "") ?? throw new Exception("Iframe src is null");
                // check if link in src attribute contain ".m3u8"
                if (iframeSrc.Contains(".m3u8"))
                {
                    movie.StreamingUrl = iframe.GetAttributeValue("src", "");
                }
                else
                {
                    // get all query params in src attribute
                    var queryParams = HttpUtility.ParseQueryString(iframeSrc) ?? throw new Exception("Query params is null");
                    // Filter all query param to get value which value start with "http"
                    var url = Array.Find(queryParams.AllKeys, x => queryParams[x].StartsWith("http")) ?? throw new Exception("Not found url in query params");
                    // Decode url
                    url = HttpUtility.UrlDecode(url);
                    // call HTTP GET to get the streaming url
                    var response = await HttpHelper.GetAsync(url);
                    // get the url contain .m3u8 in response, url must start with "http"
                    var streamingUrl = StreamingUrlRegex().Match(response).Value;
                    movie.StreamingUrl = streamingUrl;
                }
            }
            else
            {
                movie.StreamingUrl = document.DocumentNode.Descendants("iframe")
                    .FirstOrDefault(x => x.GetAttributeValue("id", "").Equals("iframe-embed"))?.GetAttributeValue("data-src", "");
            }
            return movie;
        }

        [GeneratedRegex("http.*?\\.m3u8")]
        private static partial Regex StreamingUrlRegex();
    }
}
