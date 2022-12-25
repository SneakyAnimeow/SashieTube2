using YoutubeExplode.Videos;

namespace SashieTube2.Services.YouTube;

public sealed class VideoResponse {
    public string? VideoPath { get; set; }

    public Video? VideoInfo { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public bool IsSuccess => VideoPath != null;

    public static VideoResponse Success(string videoPath, Video video) {
        return new() { VideoPath = videoPath, VideoInfo = video };
    }

    public static VideoResponse Failure(string errorMessage) {
        return new() { ErrorMessage = errorMessage };
    }
}