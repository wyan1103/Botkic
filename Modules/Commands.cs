using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord.Commands;

namespace Botkic.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        Random random = new Random();

        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync("Commands: ping, owo, uwu, anone, quote");
        }

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("._.");
        }

        [Command("uwu")]
        public async Task UwU()
        {
            await ReplyAsync("owo");
        }

        [Command("owo")]
        public async Task OwO()
        {
            await ReplyAsync("uwu");
        }

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

        [Command("quote")]
        public async Task Quote()
        {
            Quotes quotes;
            using (StreamReader file = File.OpenText(@"./Modules/quotes.json")) {
                JsonSerializer serializer = new JsonSerializer();
                quotes = (Quotes)serializer.Deserialize(file, typeof(Quotes));
            }
            int randInd = random.Next((int)quotes.MessageCount);
            await ReplyAsync(quotes.Messages[randInd].Content);
            GlobalVars.lastQuote = quotes.Messages[randInd];
        }

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
    }
}