using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;

namespace TeaPot.Bot
{
    internal class Bot
    {
        private static Bot? _instance = null;
        private DiscordSocketClient? _client;
        private BotSlashCommads _commads = new BotSlashCommads();

        public static Bot Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Bot();
                return _instance;
            }
            private set { }
        }

        private Bot() { }


        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All });

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;           
            _client.MessageReceived += HandleMessageAsync;
            _client.SlashCommandExecuted += SlashCommandHandler;            


            string botToken = TokenHolder.DISCORD_TOKEN;
            await _client.LoginAsync(TokenType.Bot, botToken);            
            await _client.StartAsync();
           

            await Task.Delay(-1);
        }


        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }


        private async Task ReadyAsync()
        {            
            Console.WriteLine($"{_client.CurrentUser.Username} is connected!");
            await _commads.TryCreateCommandsAsync(_client);
            Console.WriteLine($"Сommands have been added!");
        }


        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            BotCommandProcessorHandler.TryProcessCommand(command);            
        }


        private async Task HandleMessageAsync(SocketMessage message)
        {
            
        }
    }
}
