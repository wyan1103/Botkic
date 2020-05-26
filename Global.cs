using System;
using System.Collections.Generic;

// stores all the data gathered by the bot in the current session
static class GlobalVars
{
    public static Quotes quotes;
    public static Message lastQuote;
    public static Dictionary<string, List<string>> customQuotes;

}