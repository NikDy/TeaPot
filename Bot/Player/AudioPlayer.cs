using Discord;
using Discord.Audio;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaPot.Bot.Player
{
    internal class AudioPlayer
    {
        public ulong GuildID { get; private set; }
        public IVoiceChannel ChannelID { get; private set;}
        /// <summary>
        /// Order length
        /// </summary>
        public int Length { get => queue.Count; }

        private Queue<AudioContainer> queue = new Queue<AudioContainer>();
        private IAudioClient voiceChannelClient;
        private Task? playerTask = null;
        private AudioOutStream? audioClient;
        private CancellationTokenSource playerCancellationToken;
        private CancellationTokenSource audioCanncellationToken;
        public AudioContainer? playing { get; private set; }
        private int repeats = 0;


        public event Action Stopping;

        public AudioPlayer(ulong guildID, IVoiceChannel channelID)
        {
            GuildID = guildID;
            ChannelID = channelID;            
        }

        public void Add(AudioContainer audioContainer)
        {
            queue.Enqueue(audioContainer);
            _ = TryBufferNext();
        }

        public AudioContainer? GetNextContainer()
        {
            return queue.Peek();
        }

        public void Play()
        {
            playerCancellationToken = new CancellationTokenSource();
            playerTask = Task.Run(async () =>
            {                
                using (voiceChannelClient = await ChannelID.ConnectAsync())                    
                {                    
                    audioClient = voiceChannelClient.CreatePCMStream(AudioApplication.Music);
                    voiceChannelClient.ClientDisconnected += VoiceChannelClient_ClientDisconnected;
                    while (queue.Count > 0) 
                    {
                        playing = queue.Dequeue();
                        try
                        {
                            playing.inconsistentStream ??= await playing.Loader.Load(playing.SourceUrl);
                        }
                        catch (Exception ex) 
                        {
                            Console.WriteLine(ex.ToString());
                            continue;
                        }
                        _ = TryBufferNext();
                        audioCanncellationToken = new CancellationTokenSource();                        
                        try
                        {
                            var converted = await FFMPEGConverter.Convert(playing.inconsistentStream);
                            do
                            {
                                converted.Position = 0;
                                repeats--;
                                await converted.CopyToAsync(audioClient, audioCanncellationToken.Token);
                            }
                            while (repeats > 0);
                            playing = null;
                        }
                        catch (OperationCanceledException)
                        {
                            audioClient?.Flush();
                            playing = null;
                        }
                        if (audioClient != null) await audioClient.FlushAsync();
                    }
                }
            }, playerCancellationToken.Token);
        }

        private async Task TryBufferNext()
        {
            if (queue.Count > 0)
            {
                var next = queue.Peek();
                try
                {
                    next.inconsistentStream ??= await next.Loader.Load(next.SourceUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private Task VoiceChannelClient_ClientDisconnected(ulong arg)
        {
            PlayersHandler.DestroyPlayer(this);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Do skip in amound of times, skip including playing track
        /// </summary>
        /// <param name="times">Current track + amount of tracks to skip</param>
        public void Skip(long times = 1)
        {
            audioCanncellationToken?.Cancel();
            if (times - 1 <= 1) return;
            var toRemove = Math.Min(times - 1, queue.Count);
            for (int i = 0; i < toRemove; i++) { queue.Dequeue(); }
        }


        public void Stop()
        {
            Stopping?.Invoke();
            audioCanncellationToken?.Cancel();
            playerCancellationToken?.Cancel();
            playing = null;
            queue.Clear();
            PlayersHandler.DestroyPlayer(this);            
        }
        

        public void Repeat(long times)
        {
            repeats = (int)times;
        }

    }
}
