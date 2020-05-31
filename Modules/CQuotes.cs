using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord.Commands;

namespace Botkic.Modules
{
    public class CQuotes : ModuleBase<SocketCommandContext>
    {
        [Command("help ...")] [Alias("help quotes")]
        public async Task HelpCQuotes()
        {
            await ReplyAsync(@"**Custom Quote Commands: **
 - To add a quote: `.quoteadd [identifier] ""[Quote]""`
 - To get a list of quotes: `.quotelist [identifier]`
 - To delete a quote: `.quotedel [identifier] [index]`
 - To get a random quote: `... [identifier]`
");
        }

        // adds a new quote to the quotes dictionary
        [Command("quoteadd")]
        public async Task AddCustomQuote(string identifier, string text) {
            Dictionary<string, List<string>> dict = GlobalVars.customQuotes;
            List<string> quotes;

            // initialize the dictionary on the first addition
            if(GlobalVars.customQuotes == null) {  
                await ReplyAsync("Please do .load first");
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

        // lists all quotes associated with the identifier
        [Command("quotelist")]
        public async Task ListCustomQuote(string identifier) {
            List<string> quotes;
            if(GlobalVars.customQuotes.TryGetValue(identifier, out quotes)) {
                string response = "";
                for(int i = 0; i < quotes.Count; i++) {
                    response += $"{i+1}. \"{quotes[i]}\"\n";
                }
                response += "";
                await ReplyAsync(response);
            } else {
                await ReplyAsync($"No quotes associated with {identifier}");
            }
        }

        // deletes a quote by index using quotelist if the index is in bounds
        [Command("quotedel")]
        public async Task DelCustomQuote(string identifier, int ind) {
            List<string> quotes;
            int index = ind - 1;  // input index starts at 1
            if(GlobalVars.customQuotes.TryGetValue(identifier, out quotes)) {
                if(0 <= index && index < quotes.Count) {
                    string quote = quotes[index];
                    quotes.RemoveAt(index);
                    if(!quotes.Any()) {
                        GlobalVars.customQuotes.Remove(identifier);
                    }
                    await ReplyAsync($"Removed quote \"{quote}\"");
                } else {
                    await ReplyAsync("Segmentation fault (core dumped)");
                }
            } else {
                await ReplyAsync($"No quotes associated with {identifier}");
            }
        }

        // reply with custom quotes using the command '... *identifier* *quote*', 
        // where quotes associated with the identifier are chosen randomly
        [Command("..")]
        public async Task GetCustomQuote(string identifier) {
            List<string> quotes;
            if(GlobalVars.customQuotes != null && 
               GlobalVars.customQuotes.TryGetValue(identifier, out quotes)) 
            {
                Random random = new Random();
                string result = quotes[random.Next(quotes.Count)];
                await ReplyAsync(result);
            }
            else
            {
                await ReplyAsync($"No quotes associated with {identifier}");
            }
        }
        
        // convert custom quotes to a json file
        [Command("save")]
        public async Task Save() {
            string json = JsonConvert.SerializeObject(GlobalVars.customQuotes,
                                                      Formatting.Indented);
            File.WriteAllText(@"./MessageData/BotkicLogs/cquotes.json", json);
            await ReplyAsync("Done!");
        }

        // load custom quotes from json file
        [Command("load")]
        public async Task Load() {
            using (StreamReader file = File.OpenText(@"./MessageData/BotkicLogs/cquotes.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                GlobalVars.customQuotes = (Dictionary<string, List<string>>)
                    serializer.Deserialize (file, typeof(Dictionary<string, List<string>>));
            }
            await ReplyAsync("Done!");
        }
    }
}