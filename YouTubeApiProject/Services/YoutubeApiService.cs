using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Services
{
    public class YouTubeApiService
    {
        private readonly string _apiKey;

        public YouTubeApiService(IConfiguration configuration)
        {
            _apiKey = configuration["YouTubeApiKey"]; // Fetch API key from appsettings.json
        }

        //public async Task<List<YouTubeVideoModel>> SearchVideosAsync(string query)
        public async Task<YouTubeSearchResult> SearchVideosAsync(string query, string videoDuration = "any", string pageToken = "")
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "YoutubeProject"
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = query;  // User's query from form input
            searchRequest.MaxResults = 10;
            searchRequest.Type = "video"; //LIMIT SEARCH TO VIDEOS ONLY
            searchRequest.VideoDuration = GetVideoDurationEnum(videoDuration); //TO RETRIEVE DURATION ACCORDING TO SELECTION FROM FILTER
            searchRequest.PageToken = pageToken; //HANDLE PAGINATION

            var searchResponse = await searchRequest.ExecuteAsync();

            //GET VIDEO IDS
            var videoIds = searchResponse.Items
            .Where(item => item.Id.Kind == "youtube#video") //FILTER TO INCLUDE ONLY VIDEOS
            .Select(item => item.Id.VideoId)
            .ToList();

            if (!videoIds.Any()) return new YouTubeSearchResult();

            //FETCH ADDITIONAL DETAILS USING Videos.List
            var videoRequest = youtubeService.Videos.List("snippet,contentDetails");
            videoRequest.Id = string.Join(",", videoIds);

            var videoResponse = await videoRequest.ExecuteAsync();

            //var videos = searchResponse.Items.Select(item => new YoutubeVideoModel
            var videos = videoResponse.Items.Select(item => new YoutubeVideoModel
            {
                Title = item.Snippet.Title,
                Description = item.Snippet.Description,
                ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                VideoUrl = $"https://www.youtube.com/watch?v={item.Id}",
                UploadDate = item.Snippet.PublishedAt?.ToString("yyyy-MM-dd") ?? "Unknown",
                Duration = FormatDuration(item.ContentDetails.Duration) //THE ACTUAL DURATION OF THE VIDEO THAT GOOGLE RETURNS
            }).ToList();

            //return videos;
            return new YouTubeSearchResult
            {
                Videos = videos,
                NextPageToken = searchResponse.NextPageToken,
                PrevPageToken = searchResponse.PrevPageToken
            };
        }

        //DURATION FORMATTING
        private string FormatDuration(string isoDuration)
        {
            var duration = System.Xml.XmlConvert.ToTimeSpan(isoDuration);
            return duration.Hours > 0
                ? $"{duration.Hours}:{duration.Minutes:D2}:{duration.Seconds:D2}"
                : $"{duration.Minutes}:{duration.Seconds:D2}";
        }

        //FILTERING SEARCHING
        private SearchResource.ListRequest.VideoDurationEnum? GetVideoDurationEnum(string duration)
        {
            return duration.ToLower() switch
            {
                "short" => SearchResource.ListRequest.VideoDurationEnum.Short__,
                "medium" => SearchResource.ListRequest.VideoDurationEnum.Medium,
                "long" => SearchResource.ListRequest.VideoDurationEnum.Long__,
                _ => null // Default to 'any' (no filter)
            };
        }
    }
}