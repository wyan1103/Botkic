using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord.Commands;

namespace Botkic.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        Random random = new Random();

        // get a list of available commands
        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync("Commands: ping, owo, uwu, wood, anone, quote, quoteinfo");
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
                using (StreamReader file = File.OpenText(@"./MessageData/DiscordLogs/quotes.json")) {
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

        [Command(".. add")]
        public async Task AddCustomQuote(string identifier, [Remainder]string text) {
            Dictionary<string, List<string>> dict = GlobalVars.customQuotes;
            List<string>quotes;

            // initialize the dictionary on the first addition
            if(GlobalVars.customQuotes == null) {  
                using (StreamReader file = File.OpenText(@"./MessageData/cquotes.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    GlobalVars.customQuotes = (Dictionary<string, List<string>>)
                      serializer.Deserialize (file, typeof(Dictionary<string, List<string>>));
                }
                
                /*
                GlobalVars.customQuotes = new Dictionary<string, List<string>>();
                List<string> newQuotes = new List<string>();
                newQuotes.Add(text);
                GlobalVars.customQuotes.Add(identifier, newQuotes);
                */
            }
            // adding another quote to an existing identifier
            else if(dict.TryGetValue(identifier, out quotes)) 
            {
                quotes.Add(text);
            }
            // creating a quote for a new identifier
            else
            {
                List<string> newQuotes = new List<string>();
                newQuotes.Add(text);
                dict.Add(identifier, newQuotes);
            }
            await ReplyAsync("Added!");
        }

        // return custom quotes with the command '... *identifier* *quote*', 
        // where quotes associated with the identifier are chosen randomly
        [Command("..")]
        public async Task GetCustomQuote(string identifier) {
            List<string> quotes;
            if(GlobalVars.customQuotes != null && 
               GlobalVars.customQuotes.TryGetValue(identifier, out quotes)) 
            {
                string result = quotes[random.Next(quotes.Count)];
                await ReplyAsync(result);
            }
            else
            {
                await ReplyAsync($"No quote associated with \"{identifier}\"");
            }
        }
        // convert custom quotes to a json file
        [Command("save")]
        public async Task Save() {
            string json = JsonConvert.SerializeObject(GlobalVars.customQuotes,
                                                      Formatting.Indented);
            File.WriteAllText(@"./MessageData/cquotes.json", json);
            await ReplyAsync("Done!");
        }

        // return usage stats for the given substring
        [Command("stats")]
        public async Task Stats([Remainder]string text)
        {
            Console.WriteLine(text);
            await ReplyAsync(text);
        }
    }
}