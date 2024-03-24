using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaPot.Bot.CommandProcessor
{
    internal class RepeatCommandProcessor : ICommandProcessor
    {
        public async Task Process(SocketSlashCommand command)
        {
            Dictionary<string, object> loadedOptions = new Dictionary<string, object>();
            long times = 1;
            foreach (var option in command.Data.Options)
            {
                loadedOptions.Add(option.Name, option.Value);
            }
            if (loadedOptions.TryGetValue("times", out var amount))
            {
                times = (long)amount;
            }
            var player = PlayersHandler.TryGetAudioPlayer((ulong)command.GuildId);
            if (player.ChannelID == (command.User as IGuildUser).VoiceChannel)
            {
                if (player.Length == 0) await command.RespondAsync($"Repeating {times} times");
                player.Repeat(times);
            }
            await command.RespondAsync("Can't repeat");
        }
    }
}
