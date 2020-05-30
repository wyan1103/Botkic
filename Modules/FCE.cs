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
{    public class FCE : ModuleBase<SocketCommandContext>
    {
        // searches the FCE data for the relevant statistics, returning null if the id doesn't
        // match any course id, ("None", name) if there are no statistics for the course, and
        // (average, name) otherwise, where average is rounded to two decimal places.
        public (string, string, int)? Search (string id, bool fce, bool summer=false, int year=2018) {
            id = id.Trim();
            List<Entry> data;
            if (!GlobalVars.fceData.TryGetValue(id, out data)) {
                if (id.StartsWith("0") && !GlobalVars.fceData.TryGetValue(id.Substring(1), out data)) {
                    return null;
                }
            }

            // get average of data
            float total = 0;
            int count = 0;
            string name = "";
            foreach(Entry fceEntry in data) {
                if (fceEntry.year > year && 
                    !(summer ^ fceEntry.semester.Equals("Summer")) &&
                    !(fceEntry.section.StartsWith("W") || fceEntry.section.StartsWith("X")) &&
                    Int32.Parse(fceEntry.responseRate) > 25) {

                    float stat;
                    if (fce) stat = fceEntry.hrsPerWeek;
                    else stat = fceEntry.courseRating;

                    if (stat != -1) {
                        total += stat; 
                        count++;
                    }
                    Console.WriteLine(stat);
                }
                name = fceEntry.courseName;
            }

            if (count <= 5) {
                if (year > 2010) 
                    return Search(id, fce, summer, year-4);
                else if (count == 0)
                    return ("None", name, count);
            }
            decimal avg = (decimal) (total / count);
            decimal rounded = Math.Round(avg, 2);
            string roundedStr = string.Format("{0:0.00}", rounded);
            return (roundedStr, name, count);
        }



        [Command("dfce")]
        public async Task getFCE(string id, int year=2018) {
            var searchResults = Search(id, true, false, year);
            Console.WriteLine("OWO");
            if (searchResults == null) {
                await ReplyAsync($"No course id {id} found");
                return;
            }
            (string avg, string name, int counts) = searchResults ?? default;
            if (avg.Equals("None")) {
                await ReplyAsync($"No FCE data found for **[{id} - {name}]** since {year}.");
                return;
            }
            await ReplyAsync($"FCE average for **[{id} - {name}]** from {counts} lecture sections:  **__{avg}__**");
        }

        [Command("summer fce")]
        public async Task getSummerFCE(string id, int year=2018) {
            var searchResults = Search(id, true, true, year);
            if (searchResults == null) {
                await ReplyAsync($"No course id {id} found");
                return;
            }
            (string avg, string name, int counts) = searchResults ?? default;
            if (avg.Equals("None")) {
                await ReplyAsync($"No summer FCE data found for **[{id} - {name}]** since {year}.");
                return;
            }
            await ReplyAsync($"Summer FCE average for **[{id} - {name}]** from {counts} lecture sections:  **__{avg}__**");
        }

        [Command("rating")]
        public async Task getRating(string id, int year=2018) {
            var searchResults = Search(id, false, false, year);
            if (searchResults == null) {
                await ReplyAsync($"No course id {id} found");
                return;
            }
            (string avg, string name, int counts) = searchResults ?? default;
            if (avg.Equals("None")) {
                await ReplyAsync($"No course rating data found for **[{id} - {name}]** since {year}.");
                return;
            }
            await ReplyAsync($"Course rating average for **[{id} - {name}]** from {counts} lecture sections:  **__{avg}__**");
        }

        // one time read from the FCE csv and store in global dictionary
        [Command("dload fce")]
        public async Task loadFCE() {
            await ReplyAsync("Loading...");
            var fceDict = new Dictionary<string, List<Entry>>();
            using (var reader = new StreamReader(@"./Modules/FCEdata.csv")) {
                reader.ReadLine();  // skip first line metadata
                List<Entry> entries = new List<Entry>();
                string currentId = "";
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    Entry lineEntry = new Entry(line);

                    // add to current list or create a new list for another course
                    if (currentId.Equals(lineEntry.courseID)) {
                        entries.Add(lineEntry);
                    } else {
                        addEntry(fceDict, entries);
                        entries = new List<Entry>();
                        entries.Add(lineEntry);
                        currentId = lineEntry.courseID;
                    }
                }
            }
            GlobalVars.fceData = fceDict;
            await ReplyAsync("FCEs Loaded!");
        }

        // adds a list to a dictionary, appending it if the dictionary already contains that course id
        public Dictionary<string, List<Entry>> addEntry (Dictionary<string, List<Entry>> dict, List<Entry> elems) {
            if (elems.Count == 0) return dict;
            string id = elems[0].courseID;
            if (dict.ContainsKey(id)) {
                dict[id].AddRange(elems);
            } else {
                dict.Add(id, elems);
            }
            return dict;
        }
    }
}