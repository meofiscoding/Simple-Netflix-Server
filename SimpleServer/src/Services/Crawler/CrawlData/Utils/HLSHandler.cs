using System;
using System.Text;
using CrawlData.Helper;
using Serilog;

namespace CrawlData.Utils
{
    public static class HLSHandler
    {
        public static async Task<string> UploadHLSStream(string hlsUrl, string movieName, string? episode = null)
        {
            var client = new HttpClient();
            var playlistUri = new Uri(hlsUrl);

            // Fetch the HLS playlist
            string playlistMaster = await client.GetStringAsync(hlsUrl) ?? throw new ArgumentException("No source parameter");
            string playlist;

            // Handle case playlist is a master playlist
            if (playlistMaster.Contains("#EXT-X-STREAM-INF"))
            {
                // get URL of the media playlist then download the segments from the media playlist
                playlistUri = new Uri(playlistUri, playlistMaster.Split('\n')[2]);
                playlist = await client.GetStringAsync(playlistUri);
            }
            else
            {
                playlist = playlistMaster;
            }

            // Get everything before the last dash in playlistUri
            string playlistUriPrefix = playlistUri.AbsoluteUri[..(playlistUri.AbsoluteUri.LastIndexOf('/') + 1)];

            // Parse the playlist to extract the video segments
            ParseHLSPlaylist(playlist, playlistUriPrefix, episode == null ? $"{movieName}/hls" : $"{movieName}/{episode}/hls");
            string playlistName = (episode == null ? "" : $"{episode}/") + $"hls/{playlistUri.AbsoluteUri.Split('/').Last()}";

            // TODO: Upload the playlist to Google Cloud Storage
            string playlistUrl = GCSHelper.UploadFile(new MemoryStream(Encoding.UTF8.GetBytes(playlist)), $"{movieName}/{playlistName}");

            // TODO: Edit reference to segment in playlistMaster to point to playlistUrl
            // Replace the last line of the playlistMaster with the playlistUrl
            playlistMaster = playlistMaster[..(playlistMaster.LastIndexOf('\n') + 1)] + playlistName;

            // Get stream content from playlistMaster
            Stream playlistMasterContent = new MemoryStream(Encoding.UTF8.GetBytes(playlistMaster));

            // TODO: Upload the playlistMaster to Google Cloud Storage and return the URL
            return GCSHelper.UploadFile(playlistMasterContent, episode == null ? $"{movieName}/{hlsUrl.Split('/').Last()}" : $"{movieName}/{episode}/{hlsUrl.Split('/').Last()}");
        }

        private static void ParseHLSPlaylist(string playlist, string baseUrl, string prefix)
        {
            // example of a playlist:
            // #EXTM3U
            // #EXT-X-VERSION:3
            // #EXT-X-TARGETDURATION:8
            // #EXT-X-MEDIA-SEQUENCE:0
            // #EXTINF:7.208333,
            // c18439004b5000000.ts
            // #EXTINF:4.125000,
            // c18439004b5000001.ts
            // #EXTINF:3.416667,
            // c18439004b5000002.ts
            // #EXTINF:3.750000,
            // c18439004b5000003.ts
            // #EXTINF:4.166667,
            // #EXT-X-ENDLIST

            // Split the playlist into lines
            string[] lines = playlist.Split('\n') ?? throw new ArgumentException("No source parameter");

            // Get the segment URLs
            var segmentUrls = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // If the line starts with #EXTINF, then get the next line
                if (line.StartsWith("#EXTINF"))
                {
                    string segmentName = lines[++i];
                    var segmentUrl = new Uri(new Uri(baseUrl), segmentName).AbsoluteUri;
                    // if segment Url is incorrect (contain more than 1 http), then return
                    if (segmentUrl.Split("http").Length > 2)
                    {
                        return;
                    }
                    segmentUrls.Add(segmentUrl);
                    // TODO: Push all segment to Google Cloud Storage
                    GCSHelper.UploadFile(segmentUrl, $"{prefix}/{segmentName}");
                }
            }

            if (segmentUrls.Count == 0)
            {
                Log.Error("No segments found in playlist");
            }
        }
    }

}
