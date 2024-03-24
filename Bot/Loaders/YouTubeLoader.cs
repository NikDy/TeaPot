using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace TeaPot.Bot.Loaders
{
    internal class YouTubeLoader : ILoader
    {
        private static YoutubeClient? _youtubeClient;
        private static YouTubeLoader _youTubeLoader;
        public static YouTubeLoader Instance
        {
            get 
            {
                _youTubeLoader ??= new YouTubeLoader(); 
                return _youTubeLoader;
            }
        }

        private YouTubeLoader() { }


        public async Task<Stream?> Load(string url)
        {
            _youtubeClient ??= new YoutubeClient();
            try
            {
                var video = await _youtubeClient.Videos.GetAsync(url);
                var manifest = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
                var streamConfig = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                var audioStream = await _youtubeClient.Videos.Streams.GetAsync(streamConfig);                
                return audioStream;                
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
       

        public async Task<Video> GetVideo(string url)
        {
            _youtubeClient ??= new YoutubeClient();
            try
            {
                var video = await _youtubeClient.Videos.GetAsync(url);                
                return video;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }


        public async Task<StreamManifest> GetManifest(string url) 
        {
            _youtubeClient ??= new YoutubeClient();
            try
            {
                var video = await _youtubeClient.Videos.GetAsync(url);
                var manifest =  await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
                return manifest;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }


        public async Task<List<string>> LoadList(string url)
        {
            try
            {
                _youtubeClient ??= new YoutubeClient();
                var videoUrls = new List<string>();
                await foreach (var video in _youtubeClient.Playlists.GetVideosAsync(url))
                {
                    if (video != null) videoUrls.Add(video.Url);
                }
                return videoUrls;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
