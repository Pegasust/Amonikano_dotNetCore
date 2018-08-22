using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Amonikano.Library.AmonikanoCommands
{
    [Group("define"),Alias(new string[]
        {
            "def"
        }
        )
        ]
    public class DefineCommand: ModuleBase
    {
        [Command("life"),Summary("Shows the bot\'s interpretation of life definition")]
        public async Task DefineLife()
        {
            await ReplyAsync("42");
        }

        [Command("-auto"), Summary("Get data from google :D")]
        public async Task AutoDefine(
            [Remainder, Summary("Remainder -> value")] string value
            )
        {
            await ReplyAsync("I'm lazy to implement this feature, sorry");
        }
    }
}
