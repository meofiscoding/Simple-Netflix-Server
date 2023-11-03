using CrawlData.Helper;
using CrawlData.Model;
using FakeItEasy;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Faker;
using OpenQA.Selenium.Support.UI;

namespace Crawler.UnitTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    // naming convention: MethodName_StateUnderTest_ExpectedBehavior
    [Test]
    // Testcase 1: MovieItem have category is Movies with nested query param in iframe src
    [TestCase("https://phimmoiyyy.net/phim-le/ac-quy-ma-so-2775", "https://hd1080.opstream2.com/20231004/43610_67e76723/index.m3u8", "Ác Quỷ Ma Sơ 2 – The Nun 2 là bộ phim kinh dị siêu nhiên gothic của Hoa Kỳ được đạo diễn bởi Michael Chaves với kịch bản do Akela Cooper, Ian B. Goldberg và Richard Naing chấp bút dựa trên cốt truyện của Cooper và James Wan. Đây sẽ là phần hậu truyện của Ác quỷ ma sơ và đồng thời là phần phim thứ chín thuộc Vũ trụ The Conjuring.")]
    // Testcase 2: MovieItem have category is Movies without nested query param in iframe src
    [TestCase("https://phimmoiyyy.net/phim-le/nam-dem-kinh-hoang", "https://cdn.helvid.com/video/m3u8/202311/030f13a212c5/playlist.m3u8", "Năm Đêm Kinh Hoàng – Five Nights at Freddy’s (2023) nhân viên bảo vệ Mike bắt đầu làm việc tại Freddy Fazbear’s Pizza. Trong đêm làm việc đầu tiên, anh nhận ra mình sẽ không dễ gì vượt qua được ca đêm ở đây. Chẳng mấy chốc, anh sẽ vén màn sự thật đã xảy ra tại Freddy’s.")]
    public async Task TestCrawlMovieDetailAsync_MovieItemHaveCategoryIsMovieAndValidUrlDetail_ReturnMovieItemWithDescriptionAndStreamingUrlIsSetted(string inputUrl, string expectedStreamingUrl, string expectedDescription)
    {
        // Arrange
        MovieItem item = new MovieItem()
        {
            MovieCategory = CrawlData.Enum.Category.Movies,
            UrlDetail = inputUrl
        };
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");
        var driver = new ChromeDriver(chromeOptions);

        // Act
        var result = await CrawlHelper.CrawlMovieDetailAsync(item);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.StreamingUrl, Is.Not.Null);
            // Description must not null
            Assert.That(result.Description, Is.Not.Null);
            // StreamingUrl must equal https://hd1080.opstream2.com/20231004/43610_67e76723/index.m3u8
            Assert.That(result.StreamingUrl, Is.EqualTo(expectedStreamingUrl));
            // Description must equal
            Assert.That(result.Description, Is.EqualTo(expectedDescription));
        });
    }

    [Test]
    public void TestCrawlMovieDetailAsync_MovieURLIsNullOrEmpty_ThrowExceptionWithAppropriateMessage()
    {
        // Arrange
        MovieItem item = new()
        {
            MovieName = "Loki: Phần 2",
            UrlDetail = null
        };
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");
        var driver = new ChromeDriver(chromeOptions);
        //Act

        // Assert throw exception with message "Movie URL is null or empty"
        Assert.ThrowsAsync<Exception>(async () => await CrawlHelper.CrawlMovieDetailAsync(item), $"Movie url of movie {item.MovieName} is null or empty");
    }
}
