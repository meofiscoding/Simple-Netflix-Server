using System;
using System.Text.RegularExpressions;

namespace CrawlData.Utils
{
    public partial class CustomRegex
    {
        // url must start with "http" like "https://hd1080.opstream2.com/20231004/43610_67e76723/index.m3u8"
        // or it can be a relative path like "/20231004/43610_67e76723/index.m3u8"
        // but end with ".m3u8"
        [GeneratedRegex("(http|https)://.*.m3u8|/.*.m3u8")]
        public static partial Regex StreamingUrlRegex();
        [GeneratedRegex("\\d+")]
        public static partial Regex EpisodesRegex();
        [GeneratedRegex("^item\\s", RegexOptions.IgnoreCase, "en-VN")]
        public static partial Regex MovieItemRegex();
    }
}
