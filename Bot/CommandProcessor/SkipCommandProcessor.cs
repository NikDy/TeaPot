using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaPot.Bot.CommandProcessor
{
    internal class SkipCommandProcessor : ICommandProcessor
    {
        public async Task Process(SocketSlashCommand command)
        {
            if ((command.User as IGuildUser).VoiceChannel == null)
            {
                await command.RespondAsync("To call me you must be on a voice channel 😏");
                return;
            }
            await command.DeferAsync();
            Dictionary<string, object> loadedOptions = new Dictionary<string, object>();
            long toSkip = 1;
            foreach (var option in command.Data.Options)
            {
                loadedOptions.Add(option.Name, option.Value);
            }
            if (loadedOptions.TryGetValue("amount", out var amount))
            {
                toSkip = (long)amount;
            }
            var player = PlayersHandler.TryGetAudioPlayer((ulong)command.GuildId);
            if (player.ChannelID == (command.User as IGuildUser).VoiceChannel)
            {                
                if(player.playing == null) await command.RespondAsync("Nothing to skip");                
                player.Skip(toSkip);
                await command.ModifyOriginalResponseAsync(originalResponse => originalResponse.Content = toSkip == 1 ? "Skipping track" : $"Skippping {toSkip} tracks");
                if (player.GetNextContainer() != null) { await command.Channel.SendMessageAsync($"Now playing {player.GetNextContainer().Title} \n {player.GetNextContainer().SourceUrl}"); }
            }
            else
                await command.ModifyOriginalResponseAsync(originalResponse => originalResponse.Content = "Cant skip");
        }
    }
}
