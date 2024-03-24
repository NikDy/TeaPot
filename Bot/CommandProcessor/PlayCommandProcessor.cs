using AngleSharp.Dom;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TeaPot.Bot.Loaders;
using TeaPot.Bot.Player;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace TeaPot.Bot.CommandProcessor
{
    internal class PlayCommandProcessor : ICommandProcessor
    {
        public async Task Process(SocketSlashCommand command)
        {
            Dictionary<string, object> loadedOptions = new Dictionary<string, object>();
            foreach (var option in command.Data.Options)
            {
                loadedOptions.Add(option.Name, option.Value);
            }
            if (loadedOptions.TryGetValue("url", out var urlObject))
            {
                var url = (string)urlObject;
                if (!BotToolign.IsHttpOrHttpsValid(url))
                    await command.RespondAsync("The URL you provided to me is completely invalid or not a URL at all 😡, I can only read a valid URL");
                else
                { 
                    await command.DeferAsync();
                    if (url.Contains("list="))
                    {
                        var listTaskCancellationSource = new CancellationTokenSource();
                        var task = AddList(command, url, listTaskCancellationSource.Token);
                        _ = Task.Run(() =>
                        {
                            while (PlayersHandler.TryGetAudioPlayer((ulong)command.GuildId) == null)
                            {
                                Thread.Sleep(100);                                
                            }
                            if (PlayersHandler.TryGetAudioPlayer((ulong)command.GuildId) != null)
                                PlayersHandler.TryGetAudioPlayer((ulong)command.GuildId).Stopping += () =>
                                {
                                    listTaskCancellationSource.Cancel(); //TODO: callback system to prevent doing such things
                                    PlayersHandler.MakeGuildWait((ulong)command.GuildId);
                                };
                        });
                    }
                    else
                    {
                        await AddSingleVideo(command, url);
                    }
                }
            }
            else await command.RespondAsync("Somehow there no \"url\" option in play commad, this shouldn't have happened");
        }


        private async Task AddList(SocketSlashCommand command, string url, CancellationToken token)
        {
            var loader = YouTubeLoader.Instance;
            var list = await loader.LoadList(url);
            foreach (var videoURL in list)
            {
                if (token.IsCancellationRequested)
                {
                    var player = PlayersHandler.TryGetAudioPlayer((ulong)command.GuildId);
                    if (player != null)
                        player.Stop();
                    PlayersHandler.UnmakeGuildWait((ulong)command.GuildId);
                    return;
                }
                var manifest = await loader.GetManifest(videoURL);
                var video = await loader.GetVideo(videoURL);
                var container = new AudioContainer(videoURL,
                                              loader,
                                              (ulong)command.GuildId,
                                              (command.User as IGuildUser)?.VoiceChannel,
                                              null,
                                              video.Duration,
                                              video.Title);
                var playerResponse = PlayersHandler.TakeAudioContainer(container);
                if (playerResponse.Contains("now playing")) await command.ModifyOriginalResponseAsync(originalResponse => originalResponse.Content = PlayersHandler.TakeAudioContainer(container));
                else await command.ModifyOriginalResponseAsync(originalResponse => originalResponse.Content = PlayersHandler.TakeAudioContainer(container) + $"/{list.Count}");
            }
        }


        private async Task AddSingleVideo(SocketSlashCommand command, string url)
        {
            var video = await YouTubeLoader.Instance.GetVideo(url);
            var container = new AudioContainer(url,
                                               YouTubeLoader.Instance,
                                               (ulong)command.GuildId,
                                               (command.User as IGuildUser)?.VoiceChannel,
                                               null,
                                               video.Duration,
                                               video.Title);
            await command.ModifyOriginalResponseAsync(originalResponse => originalResponse.Content = PlayersHandler.TakeAudioContainer(container));
        }
    }
}
