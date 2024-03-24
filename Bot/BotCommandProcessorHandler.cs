using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaPot.Bot.CommandProcessor;


namespace TeaPot.Bot
{
    internal class BotCommandProcessorHandler
    {        
        public static async void TryProcessCommand(SocketSlashCommand command)
        {            
            if (!IsCommandLoaded(command.Data.Name))
            {
                await command.RespondAsync("This is weird, I don't know such a command 🤔");
                return;
            }
            switch (command.Data.Name) 
            {
                case BotSlashCommads.PLAY_COMMAND_NAME:
                    _ = new PlayCommandProcessor().Process(command);                    
                    break;
                case BotSlashCommads.STOP_COMMAND_NAME:
                    _ = new StopCommandProcessor().Process(command);                    
                    break;
                case BotSlashCommads.SKIP_COMMAND_NAME:
                    _ = new SkipCommandProcessor().Process(command);
                    break;
                case BotSlashCommads.REPEAT_COMMAND_NAME:
                    _ = new SkipCommandProcessor().Process(command);
                    break;
            }
        }


        private static bool IsCommandLoaded(string name)
        {
            return BotSlashCommads.loadedCommands.TryGetValue(name, out var command);
        }


        
                
    }
}
