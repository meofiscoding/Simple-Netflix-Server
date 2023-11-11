using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using CrawlData.Model;

namespace CrawlData.Helper
{
    public static class FileHelper
    {
        public static async Task ExtractDataToJsonAsync(List<MovieItem> movieItem)
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
    }
}
