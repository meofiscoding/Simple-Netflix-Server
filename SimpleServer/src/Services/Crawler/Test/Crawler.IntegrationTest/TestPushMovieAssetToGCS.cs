using CrawlData;
using EventBus.Message.Common.Enum;
using CrawlData.Helper;
using CrawlData.Model;
using Google.Cloud.Storage.V1;

namespace Crawler.IntegrationTest
{
    public class TestPushMovieAssetToGCS
    {
        private StorageClient storage = null!;
        [SetUp]
        public void Setup()
        {
            storage = StorageClient.Create();
        }

        // naming convention: MethodName_StateUnderTest_ExpectedBehavior
        [Test]
        public async Task TestPushMovieAssetToGCS_MovieItemWithCategoryIdMovie_AssetLocatedInCorrectFolderInGCS()
        {
            // Arrange
            MovieItem item = new()
            {
                Poster = "https://phimmoiyyy.net/wp-content/uploads/2023/09/Biet-doi-danh-thue-4.jpg",
                UrlDetail = "https://phimmoiyyy.net/phim-le/biet-doi-danh-thue-4-711415",
                MovieCategory = Category.Movies,
                Tags = new List<Tag> { Tag.Hot, Tag.Cinema },
                MovieName = "Biệt Đội Đánh Thuê 4"
            };

            // Act
            await MovieHelper.PushMovieAssetToGCS(new List<MovieItem> { item });

            // Get number of file inside folder Biet-Doi-Danh-Thuê-4/hls on GCS
            var filesInHLSFolder = storage.ListObjects("Biet-Doi-Danh-Thuê-4/hls").ToList();
            var filesInMoviesFolder = storage.ListObjects("Biet-Doi-Danh-Thuê-4").ToList();

            // Assert
            Assert.Multiple(() =>
            {
                // There must have 1590 .ts file and a .m3u8 file inside folder Biet-Doi-Danh-Thuê-4/hls
                Assert.That(filesInHLSFolder, Has.Count.EqualTo(1591));
                // There only 3 object inside folder Biet-Doi-Danh-Thuê-4
                Assert.That(filesInMoviesFolder, Has.Count.EqualTo(3));
            });
            Assert.Multiple(() =>
            {
                // There must have 1590 .ts file in filesInHLSFolder
                Assert.That(filesInHLSFolder.Count(x => x.Name.EndsWith(".ts")), Is.EqualTo(1590));
                // There only have 1 .m3u8 file in filesInHLSFolder
                Assert.That(filesInHLSFolder.Count(x => x.Name.EndsWith(".m3u8")), Is.EqualTo(1));
            });
            Assert.Multiple(() =>
            {
                // There must have 1 poster.jpg file inside folder Biet-Doi-Danh-Thuê-4
                Assert.That(filesInMoviesFolder.Count(x => x.Name.EndsWith(".jpg")), Is.EqualTo(1));
                Assert.That(filesInMoviesFolder.Count(x => x.Name.EndsWith(".m3u8")), Is.EqualTo(1));
                // There must only 1 folder hls inside folder Biet-Doi-Danh-Thuê-4
                Assert.That(filesInMoviesFolder.Count(x => x.Name == "hls"), Is.EqualTo(1));
            });
        }
    }
}
