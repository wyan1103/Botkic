using System;
using System.Collections.Generic;

// stores all the data gathered by the bot in the current session
static class GlobalVars
{
    public static string delimiter = ".";
    public static Quotes quotes;
    public static Message lastQuote;
    public static Dictionary<string, List<string>> customQuotes;
    public static Dictionary<string, List<Entry>> fceData;

}

// stores FCE entries
public class Entry {
        public int year { get; set; }
        public string semester { get; set; }
        public string courseID { get; set; }
        public string section { get; set; }
        public string professor { get; set; }
        public string courseName { get; set; }
        public string responseRate { get; set; }
        public float hrsPerWeek { get; set; }
        public float courseRating { get; set; }

        public Entry(string entryData) {
            entryData = entryData.Replace(", ", " ");
            //Console.WriteLine(entryData);
            string[] list = entryData.Split(',');
            year = Int32.Parse(list[0]);
            semester = list[1];
            courseID = list[2];
            section = list[3];
            professor = list[4];
            courseName = list[5];
            responseRate = list[6];
            try {
                hrsPerWeek = float.Parse(list[7]);
            } catch {
                hrsPerWeek = -1;
            }

            try {
                courseRating = float.Parse(list[8]);
            } catch {
                courseRating = -1;
            }
        }

        public string toString() {
            return $"ID: {courseID} \nName: {courseName} \n" + 
                   $"Rating: {courseRating} \nHours: {hrsPerWeek}";
        }
    }