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
    // modifying options for the stats and leaderboards commands
    public class StatsOptions
    {
        public bool caseSensitive { get; set; }
        public bool inclBots { get; set; }
        public bool inclSubstrings { get; set; }
        public bool ignoreRepeats { get; set; }
        public bool inclAll {get; set; }

        public StatsOptions(string tokens) {
            tokens = tokens.ToLower();
            caseSensitive = tokens.Contains("casesensitive");
            inclBots = tokens.Contains("inclbots");
            inclSubstrings = tokens.Contains("inclsubstring");
            ignoreRepeats = tokens.Contains("ignorerepeat");
            inclAll = tokens.Contains("inclall");
        }
    }

    public class StatisticsCommands : ModuleBase<SocketCommandContext>
    {
        [Command("help stats")][Alias("help leaderboard", "help logs")]
        public async Task HelpStats()
        {
            await ReplyAsync(@"**Stats, Leaderboard, and Logs Syntax:** .[stats, logs, leaderboard] [keywords] [params=None]

To search multiple keywords, separate keywords with commas and surround with quotes, e.g. `.stats ""owo, uwu""`

__Parameter Options__ (not case sensitive): 
 - inclSubstrings 
 - caseSensitive 
 - ignoreRepeats 
 - inclBots");
        }

        // regex matches a word to a given string
        public int MatchWord(string context, string word, bool substr, bool repeats) {
            if (word.Equals("")) return 1;
            Regex rx;
            if(substr){
                rx = new Regex(word);
            } else {
                string rxPattern = "";

                // don't add \b if the string begins/ends with punctuation
                if (!Char.IsPunctuation(word[0])) 
                    rxPattern += @"\b";
                rxPattern += $"{word}";
                if (!Char.IsPunctuation(word.Last()))
                    rxPattern += @"\b";
                rx = new Regex(rxPattern);
            }
            int res =  rx.Matches(context).Count;
            if (repeats && res > 0) return 1;
            return res;
        }

        // Parses a string and returns a new string after converting emotes
        // and mentions to text and converting to lowercase
        public string ParseMsg(string text, bool caseSensitive) {
            text = text.Trim();
            if(!caseSensitive) {
                text = text.ToLower();
            }
            // match emotes by <:name:id> and replace all matches with :name:
            MatchEvaluator getName = new MatchEvaluator((Match m) => m.Result("$1"));
            string pattern = @"<(:\w*:)\d*>"; 
            string res =  Regex.Replace(text, pattern, getName);
            return res;
        }

        // returns an <string:int> dictionary of users to word counts, sorted by word
        public Dictionary<string, int> GetCounts(string[] keywords, StatsOptions modifiers) {
            var counts = new Dictionary<string, int>();
            string[] allLogs = Directory.GetFiles("./MessageData/BotkicLogs/DiscordLogs", "*.json");

            // iterate through all json logs
            foreach(string filePath in allLogs) {
                Quotes logs;
                using (StreamReader file = File.OpenText(filePath)) {
                    JsonSerializer serializer = new JsonSerializer();
                    logs = (Quotes)serializer.Deserialize(file, typeof(Quotes));
                }
                // iterate through all keywords and messages within a channel 
                foreach(Message msg in logs.Messages) {
                    string content = ParseMsg(msg.Content, modifiers.caseSensitive);
                    foreach(string word in keywords) {
                        string substr = ParseMsg(word, modifiers.caseSensitive);
                        int matches = MatchWord(content, substr, modifiers.inclSubstrings, modifiers.ignoreRepeats);
                        if(matches > 0 && (modifiers.inclBots || !msg.Author.IsBot)) {
                            if(counts.ContainsKey(msg.Author.Username)) { 
                                counts[msg.Author.Username] += matches;
                            } else {
                                counts.Add(msg.Author.Username, matches);
                            }
                        }
                    }
                }
            }
            return counts;
        }

        // pads a string equally on the left and right sides with spaces
        public string PadStr(string text, int length) {
            int lenDiff = length - text.Length;
            return new string (' ', (lenDiff + 1) / 2) + text + new string (' ', lenDiff / 2);
        }

        // return usage stats for all users of the given substring
        // parameter "substr" will search for all substrings within the logs
        [Command("stats")]
        public async Task Stats(string text, [Remainder] string options="")
        {
            string[] keywords = text.Split(",");
            StatsOptions modifiers = new StatsOptions(options);
            var counts = GetCounts(keywords, modifiers);
            var orderedCounts = counts.OrderBy(kvp => -kvp.Value);  // sort usages w/ highest first
            
            int total = 0;
            foreach(KeyValuePair<string, int> entry in counts) {
                total += entry.Value;
            }
            string leaderboard = "";
            for(int i = 0; i < counts.Count; i++) {
                var (name, num) = orderedCounts.ElementAt(i);
                leaderboard += $"  {i+1}. {name}: {num}\n";
            }
            string response = $@"```c
Top 10 statistics for ""{text}"": 
{leaderboard}
Total Occurrences: {total}```";
            await ReplyAsync(response);
        }

        // returns a leaderboard of messages containing the given substring, by
        // proportion of total messages
        [Command("leaderboard")]
        public async Task Leaderboard(string text, [Remainder]string options="")
        {   
            string[] keywords = text.Split(",");
            StatsOptions modifiers = new StatsOptions(options);
            var counts = GetCounts(keywords, modifiers);
            var totals = GetCounts(new String[] {""}, modifiers);  // total message counts
            var proportions = new Dictionary<string, float>();
            
            // get total usage and total messages sent
            var (totalCounts, totalMsgs) = (0, 0);
            foreach (var entry in counts) {
                totalCounts += entry.Value;
            }
            foreach (var entry in totals) {
                totalMsgs += entry.Value;
            }

            // put proportions into a new dictionary and sort
            foreach(KeyValuePair<string, int> entry in counts) {
                var (count, total) = (0, 0);
                string name = entry.Key;
                if(counts.ContainsKey(name)) {
                    count = counts[name];
                }
                if(totals.ContainsKey(name)) {
                    total = totals[name];
                }
                proportions.Add(name, (float) count / total);
            }
            var orderedProportions = proportions.OrderBy(kvp => -kvp.Value); 

            // find the length of the longest username
            int maxNameLength = 0;
            foreach(var name in counts.Keys) {
                int len = name.Length;
                if (maxNameLength < len) 
                    maxNameLength = len;
            }

            // make and outboard the leaderboard
            string leaderboard = $" Rank | {PadStr("Username", maxNameLength)} | Total Uses | Total Msgs | Uses per 1000 Msgs \n";
            leaderboard += $"------+{new string ('-', maxNameLength + 2)}+------------+------------+--------------------\n";
            for(int i = 0; i < counts.Count; i++) {
                var (name, num) = orderedProportions.ElementAt(i);
                float ratio = 1000 * (float) counts[name] / (float) totals[name];

                string rankStr = PadStr((i+1).ToString(), 4);
                string nameStr = PadStr(name, maxNameLength);
                string countStr = PadStr(counts[name].ToString(), 10);
                string totalStr = PadStr(totals[name].ToString(), 10);
                string ratioStr = PadStr(ratio.ToString("0.00"), 18);

                leaderboard += $" {rankStr} | {nameStr} | {countStr} | {totalStr} | {ratioStr} \n";
            }
            string response = $@"```c
Usage leaderboard for ""{text}"":

{leaderboard}
Total Occurrences: {totalCounts} out of {totalMsgs} messages```";
            await ReplyAsync(response);
        }

        // returns an <string:string list> dictionary of users to messages containing the desired token
        public Dictionary<string, List<string>> GetLogs(string[] keywords, StatsOptions modifiers) {
            var counts = new Dictionary<string, List<string>>();
            string[] allLogs = Directory.GetFiles("./MessageData/BotkicLogs/DiscordLogs", "*.json");

            // iterate through all json logs
            foreach(string filePath in allLogs) {
                Quotes logs;
                using (StreamReader file = File.OpenText(filePath)) {
                    JsonSerializer serializer = new JsonSerializer();
                    logs = (Quotes)serializer.Deserialize(file, typeof(Quotes));
                }
                // iterate through all keywords and messages within a channel 
                foreach(Message msg in logs.Messages) {
                    string content = ParseMsg(msg.Content, modifiers.caseSensitive);
                    foreach(string word in keywords) {
                        string substr = ParseMsg(word, modifiers.caseSensitive);
                        int matches = MatchWord(content, substr, modifiers.inclSubstrings, modifiers.ignoreRepeats);
                        if(matches > 0 && (modifiers.inclBots || !msg.Author.IsBot)) {
                            if(counts.ContainsKey(msg.Author.Username)) { 
                                counts[msg.Author.Username].Add(msg.Content);
                            } else {
                                var msgList = new List<string>();
                                msgList.Add(msg.Content);
                                counts.Add(msg.Author.Username, msgList);
                            }
                        }
                    }
                }
            }
            return counts;
        }

        // returns a .txt file containing message logs for the given substring
        [Command("logs")]
        public async Task Logs(string text, [Remainder]string options="")
        {   
            string[] keywords = text.Split(",");
            StatsOptions modifiers = new StatsOptions(options);
            var rawLogs = GetLogs(keywords, modifiers);
            var logs = rawLogs.OrderBy(kvp => -kvp.Value.Count);

            using (StreamWriter file = new StreamWriter($@"./MessageData/Logs.txt")) {
                file.WriteLine("\n-------------------------------------------------");
                foreach(var kvp in logs) {
                    List<string> messages = kvp.Value;
                    file.WriteLine($"\nUser: {kvp.Key}\nMessages: {messages.Count}\n\n*\n");
                    foreach(var msg in messages) {
                        file.WriteLine(msg);
                        file.WriteLine("\n*\n");
                    }
                    file.WriteLine("-------------------------------------------------");
                }
            }

            var embed = new EmbedBuilder();
            await Context.Channel.SendFileAsync(@"./MessageData/Logs.txt");
        }

        // updates message logs
        [RequireOwner]
        [Command ("update")]
        public async Task Update() {
            string[] channelNames = {"announcements", "bot-spam", "general", "no-mic", "planning", 
                                     "puzzle-hunt", "quotes", "requests"};
            int total = 0;
            // key is channel name ; value is list of new messages sent in that channel
            foreach(var kvp in GlobalVars.newMessages) {
                string channel = channelNames.Contains(kvp.Key) ? kvp.Key : "misc";

                string json = File.ReadAllText($@"./MessageData/BotkicLogs/DiscordLogs/{channel}.json");
                var data = JsonConvert.DeserializeObject<Quotes>(json);
                data.Messages.AddRange(kvp.Value);
                data.MessageCount += kvp.Value.Count;
                total += kvp.Value.Count;
                string newJson = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText($@"./MessageData/BotkicLogs/DiscordLogs/{channel}.json", newJson);
            }
            GlobalVars.newMessages = new Dictionary<string, List<Message>>();
            await ReplyAsync($"Added {total} entries!");
        }
    }
}