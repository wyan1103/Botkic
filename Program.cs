using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;


namespace Botkic
{
    public class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        static void Main(string[] args) => 
            new Program().RunAsync().GetAwaiter().GetResult();

        // initial setup on bot login
        public async Task RunAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            // read the bot's discord token
            string token;
            using (StreamReader sr = new StreamReader("token.txt"))
            {
                token = sr.ReadToEnd();
            }

            _client.Log += Log;

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, token.Trim());
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        // error logs
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        // handle commands given to the bot with prefix "."
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var  msg = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, msg);
            if(msg == null) return;

            int argPos = 0;

            if (context.Guild.ToString().Trim().Equals("Wooden Bloc")) {
                SaveMessage(context);
            }

            if(!msg.Author.IsBot && msg.HasStringPrefix(GlobalVars.delimiter, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }

        private void SaveMessage(SocketCommandContext context) {
            Message newMsg = Convert(context.Message);
            if(GlobalVars.newMessages == null) {
                GlobalVars.newMessages = new Dictionary<string, List<Message>>();
            }

            var msgs = GlobalVars.newMessages;
            var channel = context.Channel.ToString();
            if(msgs.ContainsKey(channel)) {
                msgs[channel].Add(newMsg);
            } else {
                var newList = new List<Message>();
                newList.Add(newMsg);
                msgs.Add(channel, newList);
            }

            //switch (context.Channel)
            //{
                
            //}
        }

        private Message Convert(SocketUserMessage msg) {
            Message result = new Message();
            result.Id = msg.Id.ToString();
            result.Type = msg.GetType().ToString();
            result.Timestamp = msg.Timestamp;
            result.TimestampEdited = msg.EditedTimestamp;
            result.IsPinned = msg.IsPinned;
            result.Content = msg.Content;

            SocketUser user = msg.Author;
            result.Author = new Author(user.Id.ToString(), user.Username, user.Discriminator, 
                                       user.IsBot, new Uri(user.GetAvatarUrl()));
            var attachments = new List<Attachment>();
            foreach(var a in msg.Attachments) {
                attachments.Add(new Attachment(a.Id.ToString(), new Uri(a.Url), a.Filename, a.Size));
            }

            result.Attachments = attachments.ToArray();
            result.Embeds = new Object[0];
            result.Reactions = new Reaction[0];

            return result;
        } 
/*
        {
        "id": "713250020983570514",
        "type": "Default",
        "timestamp": "2020-05-22T04:40:56.547+00:00",
        "timestampEdited": null,
        "isPinned": false,
        "content": "context: \n(1) I thought it was \u0022seria\u0022 and was confusion\n(2) I read it as or-alex-am and was confusion",
        "author": {
          "id": "164104924269903872",
          "name": "ft029",
          "discriminator": "2816",
          "isBot": false,
          "avatarUrl": "https://cdn.discordapp.com/avatars/164104924269903872/e2f134d1a830e6a290e85df26771d9de.png"
        },
        "attachments": [],
        "embeds": [],
        "reactions": []
      }*/
    }
}
