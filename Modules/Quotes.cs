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
    public class QuoteCommands : ModuleBase<SocketCommandContext>
    {
        // post a random quote from #quotes
        [Command("quote")]
        public async Task Quote()
        {
            Random random = new Random();
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
                await ReplyAsync(msg.Attachments[0].Url.ToString());
            }
            GlobalVars.lastQuote = msg;
        }

        // deletes the last quote (for removing non-quotes)
        [Command("quotedelete")]
        public async Task QuoteDelete() {
            string json = File.ReadAllText($@"./MessageData/BotkicLogs/DiscordLogs/quotes.json");
            var data = JsonConvert.DeserializeObject<Quotes>(json);
            foreach(Message m in data.Messages) {
                if(m.Id == GlobalVars.lastQuote.Id) {
                    data.Messages.Remove(m);
                    data.MessageCount--;
                    await ReplyAsync("quote removed");
                    File.WriteAllText($@"./MessageData/BotkicLogs/DiscordLogs/quotes.json",
                                      JsonConvert.SerializeObject(data, Formatting.Indented));
                    return;
                }
            }
            await ReplyAsync("Couldn't remove quote");
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
                    $"Quote sent by {lastQuote.Author.Username} on {lastQuote.Timestamp.DateTime}.";
                await ReplyAsync(result);
            }
        }
    }
}