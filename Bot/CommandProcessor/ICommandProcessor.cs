using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaPot.Bot.CommandProcessor
{
    internal interface ICommandProcessor
    {
        abstract Task Process(SocketSlashCommand command);

    }
}
