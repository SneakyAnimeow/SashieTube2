namespace SashieTube2.Services.YouTube;

public interface IYouTubeService {
    Task<VideoResponse> GetVideo(string videoId);
}