using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Configuration;
using System.Collections.Generic;
using System.Threading;

namespace bot
{
    class Program
    {
        public db dbase = new db();
        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        private CommandHandler _handler;
        private EventHandler hander;

        public async Task StartAsync()
        {
            System.DateTime time = System.DateTime.Now;

            _handler = new CommandHandler(dbase.client);

            Console.WriteLine(time + "\tUruchomiono bota.");
            Thread.Sleep(5000);
            dbase.start();

            await Task.Delay(-1);
        }
    }
}
