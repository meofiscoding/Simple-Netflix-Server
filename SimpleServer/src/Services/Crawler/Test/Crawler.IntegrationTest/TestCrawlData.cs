using CrawlData.Helper;
using CrawlData.Model;
using EventBus.Message.Common.Enum;

namespace Crawler.IntegrationTest;

public class TestCrawlData
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
    [TestCase("https://phimmoiyyy.net/phim-le/nam-dem-kinh-hoang", "https://1080.opstream4.com/20231103/48864_96c231a3/index.m3u8", "Năm Đêm Kinh Hoàng – Five Nights at Freddy’s (2023) nhân viên bảo vệ Mike bắt đầu làm việc tại Freddy Fazbear’s Pizza. Trong đêm làm việc đầu tiên, anh nhận ra mình sẽ không dễ gì vượt qua được ca đêm ở đây. Chẳng mấy chốc, anh sẽ vén màn sự thật đã xảy ra tại Freddy’s.")]
    public async Task TestCrawlMovieDetailAsync_MovieItemHaveCategoryIsMovieAndValidUrlDetail_ReturnMovieItemWithDescriptionAndStreamingUrlIsSetted(string inputUrl, string expectedStreamingUrl, string expectedDescription)
    {
        // Arrange
        MovieItem item = new()
        {
            MovieCategory = Category.Movies,
            UrlDetail = inputUrl
        };

        // Act
        var result = await CrawlHelper.CrawlMovieDetailAsync(item);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.StreamingUrls, Is.Not.Null);
            // Description must not null
            Assert.That(result.Description, Is.Not.Null);
            // StreamingUrl must equal https://hd1080.opstream2.com/20231004/43610_67e76723/index.m3u8
            Assert.That(result.StreamingUrls, Has.Exactly(1).EqualTo(expectedStreamingUrl));
            // Description must equal
            Assert.That(result.Description, Is.EqualTo(expectedDescription));
        });
    }

    [Test]
    // Testcase 1: MovieItem have category is Movies have direct m3u8 link in src
    [TestCase("https://phimmoiyyy.net/phim-bo/loki-phan-2-154230",
              "Tập 5 Vietsub",
              new string[]{
                "https://hd1080.opstream2.com/20231006/43619_01b63816/index.m3u8",
                "https://hd1080.opstream2.com/20231013/43761_81b78d84/index.m3u8",
                "https://hd1080.opstream2.com/20231020/43783_004d4a5b/index.m3u8",
                "https://hd1080.opstream2.com/20231027/43792_e1102d46/index.m3u8",
                "https://hd1080.opstream2.com/20231103/43830_fd67c109/index.m3u8"
              },
              "Loki: Phần 2 – Loki Season 2 (2023) khi Steve Rogers, Tony Stark và Scott Lang quay trở về cột mốc 2012, ngay khi trận chiến ở New York kết thúc, để “mượn tạm” quyền trượng của Loki. Nhưng một tai nạn bất ngờ xảy đến, khiến Loki nhặt được khối lặp phương Tesseract và tiện thể tẩu thoát.Cuộc trốn thoát này đã dẫn đến dòng thời gian bị rối loạn. Cục TVA – tổ chức bảo vệ tính nguyên vẹn của dòng chảy thời gian, buộc phải can thiệp, đi gô cổ ông thần này về làm việc. Tại đây, Loki có hai lựa chọn, một là giúp TVA ổn định lại thời gian, không thì bị tiêu hủy. Dĩ nhiên Loki chọn lựa chọn thứ nhất. Nhưng đây là nước đi vô cùng mạo hiểm, vì ông thần này thường lọc lừa, “lươn lẹo”, chuyên đâm lén như bản tính tự nhiên của gã.")]
    // Testcase 2: MovieItem have category is Movies without m3u8 link in src
    [TestCase("https://phimmoiyyy.net/phim-bo/co-gai-mang-mat-na",
               "FULL 7/7 VIETSUB",
                new string[]{
                  "https://1080.opstream4.com/20230819/46642_594b6945/index.m3u8",
                  "https://1080.opstream4.com/20230819/46643_fd0f1bdb/index.m3u8",
                  "https://1080.opstream4.com/20230819/46644_2fe4ec5c/index.m3u8",
                  "https://1080.opstream4.com/20230819/46645_8d1badc0/index.m3u8",
                  "https://1080.opstream4.com/20230819/46646_ab94f5c7/index.m3u8",
                  "https://1080.opstream4.com/20230819/46647_c993c1df/index.m3u8",
                  "https://1080.opstream4.com/20230819/46648_b7aa0cf6/index.m3u8",
                },
                "Cô Gái Mang Mặt Nạ – Mask Girl (2023) một nhân viên văn phòng không tự tin về ngoại hình của mình trở thành nhân vật đeo mặt nạ trên mạng vào ban đêm cho đến khi một chuỗi sự kiện đen đủi ập đến với cuộc đời cô."
            )]
    public async Task TestCrawlMovieDetailAsync_MovieItemHaveCategoryIsTVShowsAndValidUrlDetail_ReturnMovieItemWithDescriptionAndStreamingUrlIsSetted(string inputUrl, string status, string[] expectedUrl, string expectedDescription)
    {
        // Arrange
        MovieItem movie = new()
        {
            MovieCategory = Category.TVShows,
            UrlDetail = inputUrl,
            Status = status
        };

        // Act
        var result = await CrawlHelper.CrawlMovieDetailAsync(movie);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.That(result.StreamingUrls, Is.Not.Null);
            // Description must not null
            Assert.That(result.Description, Is.Not.Null);
            // StreamingUrl must have number of item and each item must match by order with streamingUrls
            Assert.That(result.StreamingUrls, Has.Count.EqualTo(expectedUrl.Length));
            // Description must equal
            Assert.That(result.Description, Is.EqualTo(expectedDescription));
        });
    }

    [Test]
    public async Task TestCrawlMovieDetailAsync_MovieAlreadyExistsInDatabase_ReturnMovieItemThatNullStreamingUrlValueIsSetted()
    {
        // Arrange
        MovieItem movie = new()
        {
            MovieCategory = Category.TVShows,
            UrlDetail = "https://phimmoiyyy.net/phim-bo/cuoc-chien-sinh-ton-644141",
            Status = "Tập 13 Vietsub",
            StreamingUrls = new Dictionary<string, string>{
            {"1", ""},
            {"2", ""},
            {"3", ""},
            {"4", ""},
            {"5", "https://vie2.opstream7.com/20230930/1167_f57bfd5d/index.m3u8"},
            {"6", ""},
            {"7", ""},
            {"8", ""},
            {"9", ""},
            {"10", ""},
            {"11", "https://vie2.opstream7.com/20231028/1401_f6fc6dc9/index.m3u8"},
            {"12", ""},
            {"13", ""}
        }
        };

        // Act
        var result = await CrawlHelper.CrawlMovieDetailAsync(movie);

        // Assert
        // The url of episode 5 and 11 must be preserved
        Assert.Multiple(() =>
        {
            Assert.That(result.StreamingUrls, Is.Not.Null);
            Assert.That(result.StreamingUrls, Has.Count.EqualTo(13));
            Assert.That(result.StreamingUrls["5"], Is.EqualTo("https://vie2.opstream7.com/20230930/1167_f57bfd5d/index.m3u8"));
            Assert.That(result.StreamingUrls["11"], Is.EqualTo("https://vie2.opstream7.com/20231028/1401_f6fc6dc9/index.m3u8"));
        });
    }
}
