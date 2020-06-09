using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Discord.Commands;
using Discord;

namespace Botkic.Modules
{
    public class BasicCommands : ModuleBase<SocketCommandContext>
    {

        // get a list of available commands
        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync(
                @"**Partial List of Commands:** ping, owo, uwu, wood, anone, quote, quoteinfo, quotedelete 

 - Use `.help ...` to see custom quote commands
 - Use `.help stats` for stats and leaderboard syntax"
            );
        }

        [Command("help ...")]
        public async Task HelpCQuotes()
        {
            await ReplyAsync(@"**Custom Quote Commands: **
 - To add a quote: `.quoteadd [identifier] ""[Quote]""`
 - To get a list of quotes: `.quotelist [identifier]`
 - To delete a quote: `.quotedel [identifier] [index]`
 - To get a random quote: `... [identifier]`
");
        }

        // respond with ._.
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("._.");
        }

        // respond with 'owo'
        [Command("uwu")]
        public async Task UwU()
        {
            await ReplyAsync("owo");
        }

        // respond with 'uwu'
        [Command("owo")]
        public async Task OwO()
        {
            await ReplyAsync("uwu");
        }

        // respond with '... miss-wood'
        [Command("wood")]
        public async Task Wood()
        {
            await ReplyAsync("... miss-wood");
        }

        // post a random platelet gif <3
        [Command("anone")]
        public async Task Anone()
        {
            string[] options = {
                "https://media1.tenor.com/images/48086974f33a3e0114b2e0387f812ae4/tenor.gif?itemid=12360399",
                "https://media1.tenor.com/images/838a1763239527b040eebed862ddc1a6/tenor.gif?itemid=12300743",
                "https://gfycat.com/formalinsignificantalaskanmalamute-hataraku-saibou-platelet",
                "https://gfycat.com/clumsyexcellentleveret-cell-at-work-animegifs-platelet-applause"
            };
            Random random = new Random();
            int randInd = random.Next(4);
            await ReplyAsync(options[randInd]);
        }

        [Command ("github")]
        public async Task Github() {
            await ReplyAsync(@"https://github.com/wyan1103/Botkic");
        }

        // used for testing/debugging purposes
        [Command ("debug")]
        public async Task Debug() {
            await ReplyAsync("nothing to see here");
        }

        [RequireOwner]
        [Command ("reset debug")]
        public async Task SetDebug(bool x) {
            if (x)
                GlobalVars.delimiter = ("!");
            else
                GlobalVars.delimiter = (".");
            await ReplyAsync("OwO");
        }
    }
}