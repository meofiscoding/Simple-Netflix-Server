using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using CrawlData.Enum;
using CrawlData.Model;
using HtmlAgilityPack;

var web = new HtmlWeb();
// loading the target web page 
var document = web.Load("https://phimmoiyyy.net/");

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
        AssignMovieTag(movie, parent);
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
            MovieCategory = Enum.TryParse<Category>(item.GetAttributeValue("class", "").Split(" ").LastOrDefault(), true, out var result) ? result : Category.Movies
        };
        // if parent div id contains "featured-titles" then add tag "Hot"
        AssignMovieTag(movie, parent);
        movieItem.Add(movie);
    }
}

static void AssignMovieTag(MovieItem movie, string parent)
{
    if (parent.Contains("featured-titles") && movie.Tags.All(x => x != Tag.Hot))
    {
        movie.Tags.Add(Tag.Hot);
    }
    else if (parent.Contains("genre_phim-chieu-rap") && movie.Tags.All(x => x != Tag.Cinema))
    {
        movie.Tags.Add(Tag.Cinema);
    }
    else if (parent.Contains("dt-tvshows") && movie.Tags.All(x => x != Tag.NewSeries))
    {
        movie.Tags.Add(Tag.NewSeries);
    }
    else if (parent.Contains("dt-movies") && movie.Tags.All(x => x != Tag.NewFeaturedMovies))
    {
        movie.Tags.Add(Tag.NewFeaturedMovies);
    }
}

// crating the json output file
var json = JsonSerializer.Serialize(movieItem, new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
});

// write the data crawled to file
await File.WriteAllTextAsync("phimmoiMovieCrawledData.json", json);

Console.WriteLine($"Crawling data completed!\n {movieItem.Count} movies crawled!\n You can check output file at: {Directory.GetCurrentDirectory()}");