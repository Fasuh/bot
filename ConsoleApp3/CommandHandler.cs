using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net;
using System.Linq;
using Discord;
using System.Data;

namespace bot
{
    class CommandHandler
    {

        private DiscordSocketClient _client;

        private CommandService _service;

        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;

            _service = new CommandService();

            _service.AddModulesAsync(Assembly.GetEntryAssembly());

            _client.MessageReceived += HandleCommandAsync;
            client.UserJoined += userjoin;

        }
        private async Task HandleCommandAsync(SocketMessage s)

        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;

            var context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            if (msg.Content.Contains("discord.gg/") && ((IGuildUser)msg.Author).GetPermissions((IGuildChannel)msg.Channel).ManageChannel.Equals(true) && !msg.Channel.Name.Contains("reklam"))
            {
                adbot(s);
            }
            if (msg.HasCharPrefix('.', ref argPos) && !msg.Author.IsBot)
            {
                var result = await _service.ExecuteAsync(context, argPos);

                if (!result.IsSuccess)
                {
                    memes(s);
                }

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }
        private async Task userjoin(SocketGuildUser user)
        {
            db dbase = new db();
            if (user.Username.Contains("discord.gg/"))
            {
                await user.Guild.AddBanAsync(user);
            }
            else
            {
                string query = "SELECT w.* FROM welcome as w INNER JOIN servery as s ON w.server = s.id WHERE s.nid='" + user.Guild.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                DataTable data = dbase.Read(cmd);
                if (data.Rows[0][1] != null && data.Rows[0][2] != null)
                {
                    string tekst = data.Rows[0][2].ToString();
                    if (tekst.Contains("[user]")) tekst = tekst.Replace("[user]", user.Mention);
                    if (tekst.Contains("[server]")) tekst = tekst.Replace("[server]", user.Guild.Name);
                    await ((ISocketMessageChannel)user.Guild.GetChannel(ulong.Parse(data.Rows[0][1].ToString()))).SendMessageAsync(tekst);
                }
            }
        }
        private async void memes(SocketMessage s)
        {
            try
            {
                var msg = s as SocketUserMessage;
                var context = new SocketCommandContext(_client, msg);
                db dbase = new db();
                string meme = msg.Content.Remove(0, 1);
                string[] memen = meme.Split(' ');
                string query;
                if (msg.Channel.Name.Contains("lewd")) query = "SELECT  m.text FROM memy as m INNER JOIN `servery` as s ON m.Server = s.id WHERE m.name=@name AND s.nid='" + context.Guild.Id.ToString() + "'";
                else query = "SELECT  m.text FROM memy as m INNER JOIN `servery` as s ON m.Server = s.id WHERE m.name=@name AND s.nid='" + context.Guild.Id.ToString() + "' AND m.sfw='sfw'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                cmd.Parameters.AddWithValue("@name", memen[0]);
                if (dbase.HasRows(cmd))
                {
                    DataTable mem = dbase.Read(cmd);
                    if (mem.Rows[0][0].ToString().Contains("https://") || mem.Rows[0][0].ToString().Contains("http://"))
                    {
                        var builder = new Discord.EmbedBuilder();
                        builder.WithImageUrl(mem.Rows[0][0].ToString());
                        await s.Channel.SendMessageAsync("", false, builder);
                    }
                    else await s.Channel.SendMessageAsync(mem.Rows[0][0].ToString());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private async void adbot(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            var context = new SocketCommandContext(_client, msg);
            db dbase = new db();
            string query = "SELECT a.* FROM adbot as a INNER JOIN servery as s ON a.server = s.id WHERE s.nid='" + context.Guild.Id.ToString() + "'";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            if (dbase.HasRows(cmd))
            {
                DataTable info = dbase.Read(cmd);
                if (info.Rows[0][4].ToString() == "true")
                {
                    var kanal = ((ISocketMessageChannel)context.Guild.GetChannel(ulong.Parse(info.Rows[0][2].ToString())));
                    if (msg.Channel != kanal) await msg.DeleteAsync();
                    if (!msg.Author.IsBot)
                    {
                        await kanal.SendMessageAsync("zmutowano uzytkownika " + msg.Author.Username + " za wiadomosc ```" + msg.Content + "```");
                        var role = context.Guild.Roles.FirstOrDefault(x => x.Name == info.Rows[0][3].ToString());
                        await ((IGuildUser)msg.Author).AddRoleAsync(role);
                    }
                }
            }
        }
    }
}
