using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaPot.Bot.Loaders
{
    internal interface ILoader
    {
        public abstract Task<Stream?> Load(string url);

    }
}
