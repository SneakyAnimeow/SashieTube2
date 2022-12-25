using Microsoft.AspNetCore.Mvc;
using SashieTube2.Services.YouTube;

namespace SashieTube2.Controllers;

[ApiController]
[Route("/")]
public sealed class StreamController : ControllerBase {
    private const int ChunkSize = 1000000;

    private readonly IYouTubeService _youTubeService;

    public StreamController(IYouTubeService youTubeService) {
        _youTubeService = youTubeService;
    }

    [HttpGet]
    public ActionResult GetVideo([FromQuery(Name = "v")] string url) {
        var range = Request.Headers["Range"];
        if (range.Count < 0) return BadRequest("Requires Range header");

        var videoResult = _youTubeService.GetVideo(url).GetAwaiter().GetResult();

        if (!videoResult.IsSuccess) return BadRequest(videoResult.ErrorMessage);

        var videoPath = videoResult.VideoPath!;

        var videoSize = new FileInfo(videoPath).Length;

        var start = 0;

        if (range.Count > 1) {
            start = int.Parse(range[0]?.Split("=")[1].Split("-")[0] ?? string.Empty);
        }

        var end = Math.Min(start + ChunkSize, videoSize - 1);

        var contentLength = end - start + 1;

        Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{videoSize}");
        Response.Headers.Add("Accept-Ranges", "bytes");
        Response.Headers.Add("Content-Length", contentLength.ToString());
        Response.Headers.Add("Content-Type", "video/mp4");
        Response.StatusCode = 206;

        var videoStream = new FileStream(videoPath, FileMode.Open, FileAccess.Read, FileShare.Read, ChunkSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        videoStream.Seek(start, SeekOrigin.Begin);
        return new FileStreamResult(videoStream, "video/mp4");
    }
}