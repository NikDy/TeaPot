using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TeaPot.Bot
{
    internal class BotSlashCommads
    {
        public const string PLAY_COMMAND_NAME = "play";
        public const string SKIP_COMMAND_NAME = "skip";
        public const string STOP_COMMAND_NAME = "stop";
        public const string REPEAT_COMMAND_NAME = "repeat";


        public static Dictionary<string, SlashCommandProperties> loadedCommands { get; private set; } = new Dictionary<string, SlashCommandProperties>();

        public async Task TryCreateCommandsAsync(DiscordSocketClient client)
        {
            loadedCommands = new Dictionary<string, SlashCommandProperties>
            {
                {PLAY_COMMAND_NAME,     BuildPlay()},
                {STOP_COMMAND_NAME,     BuildStop()},
                {SKIP_COMMAND_NAME,     BuildSkip()},
                {REPEAT_COMMAND_NAME, BuildRepeat()}
            };
            var existingCommands = await client.GetGlobalApplicationCommandsAsync();           

            try
            {
                foreach (var command in loadedCommands.Values)
                {
                    if (!existingCommands.Any(x => x.Name == command.Name.Value))
                        await client.CreateGlobalApplicationCommandAsync(command);
                    else Console.WriteLine($"\"{command.Name.Value}\" is the command already exists, skipping");
                }
            }
            catch(HttpException exception)
            {               
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }


        private SlashCommandProperties BuildPlay()
        {
            SlashCommandBuilder commandBuilder = new SlashCommandBuilder();
            commandBuilder.WithName("play");
            commandBuilder.WithDescription("Play track by url");
            commandBuilder.AddOption("url", ApplicationCommandOptionType.String, "Song or playlist URL", true);
            return commandBuilder.Build();
        }


        private SlashCommandProperties BuildStop()
        {
            SlashCommandBuilder commandBuilder = new SlashCommandBuilder();
            commandBuilder.WithName("stop");
            commandBuilder.WithDescription("Stop player");
            return commandBuilder.Build();
        }


        private SlashCommandProperties BuildSkip()
        {
            SlashCommandBuilder commandBuilder = new SlashCommandBuilder();
            commandBuilder.WithName("skip");
            commandBuilder.WithDescription("Skip track");
            commandBuilder.AddOption("amount", ApplicationCommandOptionType.Integer, "How many tracks must be skipped", false);
            return commandBuilder.Build();
        }


        private SlashCommandProperties BuildRepeat()
        {
            SlashCommandBuilder commandBuilder = new SlashCommandBuilder();
            commandBuilder.WithName("repeat");
            commandBuilder.WithDescription("Set track to repeat");
            commandBuilder.AddOption("times", ApplicationCommandOptionType.Integer, "How many times should the track be repeated, if empty then infinitely", false);
            return commandBuilder.Build();
        }
    }
}
