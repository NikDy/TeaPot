using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaPot.Bot.CommandProcessor
{
    internal class StopCommandProcessor : ICommandProcessor
    {
        public async Task Process(SocketSlashCommand command)
        {
            if ((command.User as IGuildUser).VoiceChannel == null)
            {
                await command.RespondAsync("To call me you must be on a voice channel 😏");
                return;
            }
            var player = PlayersHandler.TryGetAudioPlayer((ulong)command.GuildId);
            if (player != null && player.ChannelID == (command.User as IGuildUser).VoiceChannel)
            {
                player.Stop();
                command.RespondAsync("Stopping");
            }
            command.RespondAsync("Already stopped");
        }
    }
}
