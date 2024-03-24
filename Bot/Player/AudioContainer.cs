using Discord;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaPot.Bot.Loaders;

namespace TeaPot.Bot.Player
{
    internal class AudioContainer
    {        

        public string SourceUrl { get; private set; }
        public ILoader Loader { get; private set; }
        public ulong TargetGuildId { get; private set; }
        public IVoiceChannel TargetAudioChannelId { get; private set; }
        public Stream? inconsistentStream = null;
        public string? Container = null;
        public TimeSpan? Duration { get; private set; }
        public string Title { get; private set; } = "No name";

        public AudioContainer(string url, ILoader loader, ulong guildID, IVoiceChannel audioChannelID, Stream stream, TimeSpan? duraion, string title = "No name")
        {
            SourceUrl = url;
            Loader = loader;
            TargetGuildId = guildID;
            TargetAudioChannelId = audioChannelID;
            inconsistentStream = stream;
            Duration = duraion;
            Title = title;
        }

        
    }
}
