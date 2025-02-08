namespace YouTubeApiProject.Models
{
    public class YouTubeSearchResult
    {
        public List<YoutubeVideoModel> Videos { get; set; }
        public string NextPageToken { get; set; }
        public string PrevPageToken { get; set; }

    }
}