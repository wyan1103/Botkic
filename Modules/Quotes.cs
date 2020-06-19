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

            int ind = random.Next((int)GlobalVars.quotes.MessageCount);
            var allMsgs = GlobalVars.quotes.Messages;

            var output = new List<Message>();

            // add all messages sent shortly before target to the output list
            int tempInd = ind - 1;
            while(tempInd >= 0) {
                TimeSpan timeDiff = allMsgs[ind].Timestamp - allMsgs[tempInd].Timestamp;
                if (Math.Abs(timeDiff.TotalMinutes) <= 1) 
                    output.Add(allMsgs[tempInd]);
                else
                    break;
                tempInd--;
            }

            // add target message to output list
            output.Add(GlobalVars.quotes.Messages[ind]);

            // add all messages sent shortly after target to the output list
            tempInd = ind + 1;
            while(tempInd < allMsgs.Count) {
                TimeSpan timeDiff = allMsgs[tempInd].Timestamp - allMsgs[ind].Timestamp;
                if (Math.Abs(timeDiff.TotalMinutes) <= 1) 
                    output.Add(allMsgs[tempInd]);
                else
                    break;
                tempInd++;
            }

            foreach(Message m in output) {
                // return text, if it exists
                if (!String.IsNullOrEmpty(m.Content)) {
                    await ReplyAsync(m.Content);
                }
                // return any attachment (image), if it exists
                if (!(m.Attachments == null || m.Attachments.Length == 0)) 
                {
                    await ReplyAsync(m.Attachments[0].Url.ToString());
                }
            }
            GlobalVars.lastQuote = allMsgs[ind];
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