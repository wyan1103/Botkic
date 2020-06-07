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
    public class StatisticsCommands : ModuleBase<SocketCommandContext>
    {
        [Command("help stats")][Alias("help leaderboard")]
        public async Task HelpStats()
        {
            await ReplyAsync(@"**Stats and Leaderboard Syntax:** .stats [keywords] [params=None] or .leaderboard [keyword] [params=None]

To search multiple keywords, separate keywords with commas and surround with quotes, e.g. `.stats ""owo, uwu""`

__Parameter Options__ (not case sensitive): 
 - inclSubstrings 
 - caseSensitive 
 - ignoreRepeats 
 - inclBots

Ex. 
.stats owo inclBots ignoreRepeats  -->  ""thowo"" and ""owo owo"" both count as one usage
.leaderboard UwU caseSensitive     -->  ""uwu"" is ignored");
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

        // returns an <string:int> dictionary of users to word counts, worted by word
        public Dictionary<string, int> GetCounts(string[] keywords, bool[] options) {
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
                    string content = ParseMsg(msg.Content, options[0]);
                    foreach(string word in keywords) {
                        string substr = ParseMsg(word, options[0]);
                        int matches = MatchWord(content, substr, options[2], options[3]);
                        if(matches > 0 && (options[1] || !msg.Author.IsBot)) {
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

        // parses optional parameters given for stats/leaderboard commands into a bool array
        public bool[] ParseParams(string p) {
            var result = new bool[4];
            p = p.ToLower();
            result[0] = p.Contains("casesensitive");
            result[1] = p.Contains("inclbots");
            result[2] = p.Contains("inclsubstrings");
            result[3] = p.Contains("ignorerepeats");
            return result;
        }

        // return usage stats for all users of the given substring
        // parameter "substr" will search for all substrings within the logs
        [Command("stats")]
        public async Task Stats(string text, [Remainder] string options="")
        {
            string[] keywords = text.Split(",");
            var counts = GetCounts(keywords, ParseParams(options));
            var orderedCounts = counts.OrderBy(kvp => -kvp.Value);  // sort usages w/ highest first
            
            int total = 0;
            foreach(KeyValuePair<string, int> entry in counts) {
                total += entry.Value;
            }
            string leaderboard = "";
            for(int i = 0; i < 10 && i < counts.Count; i++) {
                var (name, num) = orderedCounts.ElementAt(i);
                leaderboard += $"  {i+1}. {name}: {num}\n";
            }
            string response = $@"```c
Top 10 statistics for ""{text}"": 
{leaderboard}
Total Occurrences: {total}```";
            await ReplyAsync(response);
        }

        [Command("leaderboard")]
        public async Task Leaderboard(string text, [Remainder]string options="")
        {   
            Console.WriteLine(text);
            string[] keywords = text.Split(",");
            var counts = GetCounts(keywords, ParseParams(options));
            var totals = GetCounts(new String[] {""}, ParseParams(options));  // total message counts
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

            // make and outboard the leaderboard
            string leaderboard = "";
            for(int i = 0; i < 10 && i < counts.Count; i++) {
                var (name, num) = orderedProportions.ElementAt(i);
                leaderboard += $"  {i+1}. {name}: {counts[name]}|{totals[name]}\n";
            }
            string response = $@"```c
Top 10 relative statistics for ""{text}"": 
{leaderboard}
Total Occurrences: {totalCounts}|{totalMsgs}```";
            await ReplyAsync(response);
        }
    }
}