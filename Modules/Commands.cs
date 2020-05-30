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
    public class Commands : ModuleBase<SocketCommandContext>
    {
        Random random = new Random();

        // get a list of available commands
        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync(
                @"**Commands:** ping, owo, uwu, wood, anone, quote, quoteinfo, stats 

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
            int randInd = random.Next(4);
            await ReplyAsync(options[randInd]);
        }

        // post a random quote from #quotes
        [Command("quote")]
        public async Task Quote()
        {
            // read from the json file on the first quotes command
            if (GlobalVars.quotes == null) {
                using (StreamReader file = File.OpenText(@"./MessageData/BotkicLogs/DiscordLogs/quotes.json")) {
                    JsonSerializer serializer = new JsonSerializer();
                    GlobalVars.quotes = (Quotes)serializer.Deserialize(file, typeof(Quotes));
                }
            }
            int randInd = random.Next((int)GlobalVars.quotes.MessageCount);
            Message msg = GlobalVars.quotes.Messages[randInd];
            // return text, if it exists
            if (!String.IsNullOrEmpty(msg.Content)) {
                await ReplyAsync(msg.Content);
            }
            // return any attachment (image), if it exists
            if (!(msg.Attachments == null || msg.Attachments.Length == 0)) 
            {
                await ReplyAsync(msg.Attachments[0].Url);
            }
            GlobalVars.lastQuote = msg;
        }

        // get the poster and timestamp of the previous quote
        [Command("quoteinfo")]
        public async Task QuoteInfo()
        {
            Message lastQuote = GlobalVars.lastQuote;
            if (lastQuote == null) {
                await ReplyAsync("Get a quote with .quote first!");
            }
            else {
                string result =
                    $"Quote sent by {lastQuote.Author.Name} on {lastQuote.Timestamp.DateTime}.";
                await ReplyAsync(result);
            }
        }

        [Command ("github")]
        public async Task Github() {
            await ReplyAsync(@"https://github.com/wyan1103/Botkic");
        }

        // used for testing/debugging purposes
        [Command ("debug")]
        public async Task Debug() {
            Console.WriteLine(GlobalVars.fceData["15150"][0].toString());
        }
    }
}