using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

using Timer = System.Timers.Timer;

namespace SashieTube2.Services.YouTube;

public sealed class YouTubeService : IYouTubeService {
    private readonly YoutubeClient _youtubeClient;

    public YouTubeService() {
        _youtubeClient = new YoutubeClient();
    }

    public async Task<VideoResponse> GetVideo(string link) {
        VideoResponse output;

        try {
            var video = await _youtubeClient.Videos.GetAsync(link);
            var videoFileName = video.Id + ".mp4";
            var videoFilePath = Path.Combine("videos", videoFileName);

            if (!File.Exists(videoFilePath)) {
                var manifest = await _youtubeClient.Videos.Streams.GetManifestAsync(link);
                var streamInfo = manifest.GetMuxedStreams().Where(s => !s.VideoQuality.IsHighDefinition)
                    .GetWithHighestVideoQuality();

                await _youtubeClient.Videos.Streams.DownloadAsync(streamInfo, videoFilePath);
                
                //delete the video in 12 hours
                var timer = new Timer(12 * 60 * 60 * 1000);
                timer.Elapsed += (sender, args) => {
                    File.Delete(videoFilePath);
                    timer.Stop();
                };
                timer.Start();
            }

            output = VideoResponse.Success(videoFilePath, video);
        }
        catch (Exception e) {
            output = VideoResponse.Failure(e.Message);
        }

        return output;
    }
}