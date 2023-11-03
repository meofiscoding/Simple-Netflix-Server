using CrawlData.Helper;
using CrawlData.Model;

namespace Crawler.UnitTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    // naming convention: MethodName_StateUnderTest_ExpectedBehavior
    [Test]
    public void TestCrawlMovieDetailAsync_MovieItemHaveCategoryIsMovieAndIframeSrcContainM3U8_ReturnMovieItemWithDescriptionAndStreamingUrlIsSetted()
    {
        // Arrange
        MovieItem item = new MovieItem()
        {
            MovieCategory = CrawlData.Enum.Category.Movies,
            UrlDetail = "https://phimmoiyyy.net/phim-le/ac-quy-ma-so-2775"
        };

        // Act
        var result = CrawlHelper.CrawlMovieDetailAsync(item);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result.StreamingUrl, Is.Not.Null);
            // Description must not null
            Assert.That(result.Result.Description, Is.Not.Null);
            // StreamingUrl must equal https://hd1080.opstream2.com/20231004/43610_67e76723/index.m3u8
            Assert.That(result.Result.StreamingUrl, Is.EqualTo("https://hd1080.opstream2.com/20231004/43610_67e76723/index.m3u8"));
            // Description must equal
            Assert.That(result.Result.Description, Is.EqualTo("Ác Quỷ Ma Sơ 2 - The Nun 2 là bộ phim kinh dị siêu nhiên gothic của Hoa Kỳ được đạo diễn bởi Michael Chaves với kịch bản do Akela Cooper, Ian B. Goldberg và Richard Naing chấp bút dựa trên cốt truyện của Cooper và James Wan. Đây sẽ là phần hậu truyện của Ác quỷ ma sơ và đồng thời là phần phim thứ chín thuộc Vũ trụ The Conjuring."));
        });
    }
}
