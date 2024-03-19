using Discord;
using Discord.Audio;
using Discord.WebSocket;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TeaPot;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using static System.Net.Mime.MediaTypeNames;

public class Program
{
    private DiscordSocketClient _client;
    private YoutubeClient _youtubeClient;

    public static async Task Main(string[] args)
    {
        await new Program().RunBotAsync();
    }

    public async Task RunBotAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig{ GatewayIntents = GatewayIntents.All});
        _youtubeClient = new YoutubeClient();

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += HandleMessageAsync;


        string botToken = TokenHolder.DISCORD_TOKEN;
        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();
        


        await Task.Delay(-1);
    }

    private async Task ReadyAsync()
    {
        Console.WriteLine($"{_client.CurrentUser.Username} is connected!");
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    public async Task<Stream> ConvertWebMToPCM(Stream audioStream)
    {
        try
        {
            var tempFilePath = Path.GetTempFileName();
            using (FileStream fs = File.OpenWrite(tempFilePath)) 
            {
                audioStream.CopyTo(fs);
            }

            Process CreateStream(string path)
            {
                return Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                });
            }

            using (var ffmpeg = CreateStream(tempFilePath))
            using (var output = ffmpeg.StandardOutput.BaseStream)
                return output;


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    //public Stream ConvertWebMToPCM(Stream audioStream)
    //{
    //    try
    //    {
    //        // Создаем временный файл
    //        var tempFilePath = Path.GetTempFileName();
    //
    //        // Записываем входной поток во временный файл
    //        using (var fileStream = File.OpenWrite(tempFilePath))
    //        {
    //            audioStream.CopyTo(fileStream);
    //        }
    //
    //        // Читаем временный файл с помощью AudioFileReader
    //        using (var reader = new AudioFileReader(tempFilePath))
    //        {
    //            var format = new WaveFormat(48000, 2); // PCM format, 44.1kHz, stereo
    //            using (var resampler = new MediaFoundationResampler(reader, format))
    //            {
    //                var outputStream = new MemoryStream();
    //                WaveFileWriter.WriteWavFileToStream(outputStream, resampler);
    //                outputStream.Position = 0;
    //
    //                // Удаляем временный файл
    //                File.Delete(tempFilePath);
    //
    //                return outputStream;
    //            }
    //        }
    //
    //    }
    //    catch (Exception ex) { Console.WriteLine(ex); }
    //    return null;
    //}


    bool isPlaying = false;
    Queue<Video> queue = new Queue<Video>();
    

    CancellationTokenSource tokenSource = null;
    CancellationToken token;
    IAudioClient voiceChannelClient;

    private Process CreateProcess(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
    }

    private async Task HandleMessageAsync(SocketMessage message)
    {
        if (!(message is SocketUserMessage userMessage)) return;
        if (userMessage.Author.IsBot) return;


        var content = userMessage.Content;
        var voiceChannel = (userMessage.Author as IGuildUser)?.VoiceChannel;

        if (content.StartsWith("!play") && voiceChannel != null)
        {            
            if (!isPlaying)
            {                                                
                var _ = Task.Run(async () =>
                {
                    try
                    {
                        queue.Enqueue(await _youtubeClient.Videos.GetAsync(content.Substring("!play".Length).Trim()));
                        using (voiceChannelClient = await voiceChannel.ConnectAsync())
                        {
                            isPlaying = true;
                            var audioClient = voiceChannelClient.CreatePCMStream(AudioApplication.Music);
                            while (queue.Count > 0 && isPlaying)
                            {
                                var video = queue.Dequeue();
                                await message.Channel.SendMessageAsync($"Сейчас играет {video.Title}");
                                var url = video.Url;
                                var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(url);
                                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                                if (audioStreamInfo != null)
                                {
                                    var audioStream = await _youtubeClient.Videos.Streams.GetAsync(audioStreamInfo);
                                    tokenSource = new CancellationTokenSource();
                                    try
                                    {
                                        var tempFilePath = Path.GetTempFileName();
                                        using (FileStream fs = File.OpenWrite(tempFilePath))
                                        {
                                            audioStream.CopyTo(fs);
                                        }
                                        using (var ffmpeg = CreateProcess(tempFilePath))
                                        {
                                            await ffmpeg.StandardOutput.BaseStream.CopyToAsync(audioClient, tokenSource.Token);
                                        }
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        Console.WriteLine(audioClient == null);
                                    }
                                    if (audioClient != null) await audioClient.FlushAsync();
                                }
                            }
                            isPlaying = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}");
                    }
                });
            }
            else
            {
                var url = content.Substring("!play".Length).Trim();
                var video = await _youtubeClient.Videos.GetAsync(url);
                queue.Enqueue(video);                
                await message.Channel.SendMessageAsync($"{video.Title} добавлен в очередь, позиция {queue.Count}");
            }
        }
        else if(content.StartsWith("!stop") && voiceChannel != null)
        {
            await message.Channel.SendMessageAsync($"Отключаюсь");
            if (message.Author.Username != "t3adragon") await message.Channel.SendMessageAsync($"Пошел нахуй {message.Author.GlobalName}");
            queue.Clear();
            isPlaying = false;
            tokenSource.Cancel();           
            tokenSource.Dispose();
            await voiceChannelClient.StopAsync();
        }
        else if (content.StartsWith("!skip") && voiceChannel != null)
        {
            if (!isPlaying)
            {
                await message.Channel.SendMessageAsync($"Нечего пропускать");
                return;
            }
            if (queue.Count == 0)
            {
                isPlaying = false;
                await message.Channel.SendMessageAsync($"В очереди больше ничего не осталось");
            }
            else
            {
                await message.Channel.SendMessageAsync($"В очереди осталось {queue.Count} треков");
            }
            tokenSource.Cancel(true);
            tokenSource.Dispose();
        }
    }
}
