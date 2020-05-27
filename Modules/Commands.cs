using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
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

        // load custom quotes from json file
        [Command("load")]
        public async Task Load() {
            using (StreamReader file = File.OpenText(@"./MessageData/cquotes.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                GlobalVars.customQuotes = (Dictionary<string, List<string>>)
                    serializer.Deserialize (file, typeof(Dictionary<string, List<string>>));
            }
            await ReplyAsync("Done!");
        }

        // helper function that returns whether or not a word occurs as an
        // independent word (not a substring) within a message
        public bool IsIndependent(string context, string word) {
            if (word.Equals("")) return true;
            int index = context.IndexOf(word);
            while(index != -1) {
                bool left = true;
                bool right = true;
                if (index > 0) {
                    left = !Char.IsLetter(context[index-1]);
                }
                if (index < context.Length - word.Length) {
                    right = !Char.IsLetter(context[index + word.Length]);
                }
                if (left && right) return true;
                index = context.IndexOf(word, index + word.Length);
            }
            return false;
        }
/*
        // return usage stats for a single user of the given substring
        [Command("stats")]
        public async Task Stats(string username, string text, string method) {
            string[] logs = Directory.GetFiles("./MessageData/DiscordLogs", "*.json");
            foreach(string filePath in logs) {
                Quotes messages;
                using (StreamReader file = File.OpenText(filePath)) {
                    JsonSerializer serializer = new JsonSerializer();
                    messages = (Quotes)serializer.Deserialize(file, typeof(Quotes));
                }
            }
        }
*/
        // return usage stats for all users of the given substring
        // parameter "substr" will search for all substrings within the logs
        [Command("stats")]
        public async Task Stats(string substr, string method = "nosubstr")
        {
            var counts = new Dictionary<string, int>();
            bool inclSubstrings = method.Equals("includesubstrings");
            string[] allLogs = Directory.GetFiles("./MessageData/DiscordLogs", "*.json");

            // iterate through all json logs
            foreach(string filePath in allLogs) {
                Quotes logs;
                using (StreamReader file = File.OpenText(filePath)) {
                    JsonSerializer serializer = new JsonSerializer();
                    logs = (Quotes)serializer.Deserialize(file, typeof(Quotes));
                }
                // iterate through all messages within a channel 
                foreach(Message msg in logs.Messages) {
                    if((inclSubstrings && msg.Content.Contains(substr)) ||  // search for substrings
                        IsIndependent(msg.Content.ToLower(), substr)) {     // search for indep words
                        if(counts.ContainsKey(msg.Author.Name)) {
                            counts[msg.Author.Name]++;
                        } else {
                            counts.Add(msg.Author.Name, 1);
                        }
                        // Console.WriteLine(msg.Content);
                    }
                }
            }
            var orderedCounts = counts.OrderBy(kvp => -kvp.Value); // sort usages w/ highest first
            int total = 0;
            foreach(KeyValuePair<string, int> entry in counts) {
                total += entry.Value;
            }

            string leaderboard = "";
            for(int i = 0; i < 10; i++) {
                var (name, num) = orderedCounts.ElementAt(i);
                leaderboard += $"  {i+1}. {name}: {num}\n";
            }
            string q = "\"";
            string response = $@"```c
Top 10 statistics for {q}{substr}{q}: 
{leaderboard}
Total Occurances: {total}```";
            await ReplyAsync(response);
        }
    }
}