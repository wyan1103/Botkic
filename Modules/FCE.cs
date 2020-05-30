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
        [Command("fce")]
        public async Task getFCE(string id, int year=2018) {
            id = id.Trim();
            List<Entry> data;
            if (!GlobalVars.fceData.TryGetValue(id, out data)) {
                if (id.StartsWith("0") && !GlobalVars.fceData.TryGetValue(id.Substring(1), out data)) {
                    await ReplyAsync($"No course id {id} found");
                }
            }

            // get average of FCEs
            float total = 0;
            int count = 0;
            string name = "";
            foreach(Entry fceEntry in data) {
                if (fceEntry.year > year && !fceEntry.semester.Equals("Summer") &&
                        !(fceEntry.section.StartsWith("W") || fceEntry.section.StartsWith("X")) &&
                        fceEntry.hrsPerWeek != -1) {
                    total += fceEntry.hrsPerWeek; 
                    count++;
                }
                name = fceEntry.courseName;
            }

            // if there is no fce data, try pushing back the year and if that doesn't work return an error msg
            if(count == 0) {
                if (year > 2010)
                    await getFCE(id, year - 4);
                else
                    await ReplyAsync($"No FCE data found for **[{id} - {name}]** since {year}.");
                return;
            }

            decimal avg = (decimal) (total / count);
            decimal rounded = Math.Round(avg, 2);
            string roundedStr = string.Format("{0:0.00}", rounded);
            await ReplyAsync($"FCE average for **[{id} - {name}]** since {year}:  **__{roundedStr}__**");
        }

        [Command("summer fce")]
        public async Task getSummerFCE(string id, int year=2018) {
            id = id.Trim();
            List<Entry> data;

            if (!GlobalVars.fceData.TryGetValue(id, out data)) {
                if (id.StartsWith("0") && !GlobalVars.fceData.TryGetValue(id.Substring(1), out data)) {
                    await ReplyAsync($"No course id {id} found");
                }
            }

            // get average of FCEs
            float total = 0;
            int count = 0;
            string name = "";
            foreach(Entry fceEntry in data) {
                if (fceEntry.year >= year && fceEntry.semester.Equals("Summer") &&
                        !(fceEntry.section.StartsWith("W") || fceEntry.section.StartsWith("X")) &&
                        fceEntry.hrsPerWeek != -1) {
                    total += fceEntry.hrsPerWeek; 
                    count++;
                }
                name = fceEntry.courseName;
            }

            // if there is no fce data, try pushing back the year and if that doesn't work return an error msg
            if(count == 0) {
                if (year > 2010)
                    await getSummerFCE(id, year - 4);
                else
                    await ReplyAsync($"No Summer FCE data found for **[{id} - {name}]** since {year}.");
                return;
            }

            decimal avg = (decimal) (total / count);
            decimal rounded = Math.Round(avg, 2);
            string roundedStr = string.Format("{0:0.00}", rounded);
            await ReplyAsync($"Summer FCE average for **[{id} - {name}]** since {year}:  **__{roundedStr}__**");
        }

        [Command("rating")]
        public async Task getRating(string id, int year=2018) {
            id = id.Trim();
            List<Entry> data;
            if (!GlobalVars.fceData.TryGetValue(id, out data)) {
                if (id.StartsWith("0") && !GlobalVars.fceData.TryGetValue(id.Substring(1), out data)) {
                    await ReplyAsync($"No course id {id} found");
                }
            }

            // get average of ratings
            float total = 0;
            int count = 0;
            string name = "";
            foreach(Entry fceEntry in data) {
                if (fceEntry.year > year && !fceEntry.semester.Equals("Summer") &&
                        !(fceEntry.section.StartsWith("W") || fceEntry.section.StartsWith("X")) &&
                        fceEntry.courseRating != -1) {
                    total += fceEntry.courseRating; 
                    count++;
                }
                name = fceEntry.courseName;
            }

            // if there is no fce data, try pushing back the year and if that doesn't work return an error msg
            if(count == 0) {
                if (year > 2010)
                    await getRating(id, year - 4);
                else
                    await ReplyAsync($"No rating data found for **[{id} - {name}]** since {year}.");
                return;
            }

            decimal avg = (decimal) (total / count);
            decimal rounded = Math.Round(avg, 2);
            string roundedStr = string.Format("{0:0.00}", rounded);
            await ReplyAsync($"Course Rating average for **[{id} - {name}]** since {year}:  **__{roundedStr}__**/5.00");
        }

        // one time read from the FCE csv and store in dictionary
        [Command("load fce")]
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