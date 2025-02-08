using Microsoft.AspNetCore.Mvc;
using YouTubeApiProject.Services;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Controllers
{
    public class YouTubeController : Controller
    {
        private readonly YouTubeApiService _youtubeService;

        public YouTubeController(YouTubeApiService youtubeService)
        {
            _youtubeService = youtubeService;

        }

        // Display Search Page
        public IActionResult Index()
        {
            //return View(new List<YoutubeVideoModel>()); // Pass an empty list initially
            return View(new YouTubeSearchResult
            {
                Videos = new List<YoutubeVideoModel>() 
            });
        }

        // Handle the search query
        [HttpPost]
        public async Task<IActionResult> Search(string query, string videoDuration = "any", string pageToken = "")
        {
            var videos = await _youtubeService.SearchVideosAsync(query, videoDuration, pageToken);

            // Store query and duration in ViewData for form persistence
            ViewData["query"] = query;
            ViewData["videoDuration"] = videoDuration;

            return View("Index", videos);
        }
    }
}