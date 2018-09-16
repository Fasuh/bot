using System.Threading.Tasks;
using System;
using System.Net;
using System.Data;
using System.IO;
using Discord;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Collections.Generic;


namespace bot
{
    public class Test : ModuleBase<SocketCommandContext>
    {
        db dbase = new db();
        public System.DateTime time = System.DateTime.Now;
        public IReadOnlyCollection<SocketGuildUser> Users { get; }
        [Command("kick")]
        [RequireBotPermission(Discord.GuildPermission.KickMembers)]
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        public async Task KickAsync(Discord.IGuildUser user, [Remainder] string reason)
        {
            if (user.GuildPermissions.KickMembers)
            {
                var b = new Discord.EmbedBuilder();
                b.WithTitle("User Kicked");
                b.WithDescription(user.Username + "was kicked.");
                b.WithColor(new Discord.Color(0, 170, 255));
                await Context.Channel.SendMessageAsync("", false, b);
                await user.KickAsync();
            }
        }
        [Command("help")]
        public async Task help()
        {
            var builder = new Discord.EmbedBuilder();

            builder.WithTitle("Komendy");
            builder.AddInlineField("Memy", "`.dmem <nazwa> <link>` - Dodaje mema na serwer,\n\u200b`.dmemnsfw <nazwa> <link>` - Dodaje mema na serwer **tylko nsfw**,\n\u200b`.umem <nazwa>` - Usuwa mem z serwera,\n\u200b`.memelist` - Pokazuje listę memów");
            builder.AddInlineField("Warny", "`.warn <osoba> <powód>` - Daje warna osobie,\n\u200b`.delwarn <osoba> <numer warna>` - Usuwa wybrany warn,\n\u200b`.clearwarns <osoba>` - Czysci wszystkie warny osoby,\n\u200b`.listwarns <[opcjonalnie]osoba>` - pokazuje twoje/czyjes warny.");
            builder.AddInlineField("Rangi", "`.drank <nazwa>` - range o podanej nazwie.,\n\u200b`.urank <nazwa>` - usuwa range podanej nazwie,\n\u200b`.ranga <nazwa>` - nadaje range o podanej nazwie,\n\u200b`.rangi` - pokazuje spis rang dostępnych na serwerze");
            builder.AddInlineField("Fun", "`.patyk <osoba>` - narzędzie wzajemnej motywacji,\n\u200b`.rate <link/zlacznik>` - postuje obrazek na wyznaczonym kanale do oceny przez innych uzytkownikow,\n\u200b`.info <[opcjonalnie]osoba>` - podaje informacje z twojego profilu\n\u200b`.da <link>` - zapisuje link do twojego Deviantartu\n\u200b`.pokazda <[opcjonalnie]osoba>` - podaje link do twojego lub czyjegoś Deviantartu.");
            builder.AddInlineField("Powitania", "`.welmsg <wiadomosc>` - wiadomosc powitalna,\n\u200b`.welchan <kanal>` - kanal na którym wysyłana będzie wiadomość.");
            builder.WithColor(Color.DarkGreen);
            await Context.Channel.SendMessageAsync("", false, builder);
        }

        [Command("rules")]
        public async Task rules()
        {
            var b = new Discord.EmbedBuilder();
            b.WithTitle("Rules");
            b.WithDescription("-WOAH");

            b.WithColor(new Discord.Color(0, 170, 255));
            await Context.Channel.SendMessageAsync("", false, b);
        }
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        [Command("test")]
        public async Task test()
        {
            try
            {
                IReadOnlyCollection<Discord.WebSocket.SocketGuildUser> c = Context.Guild.Users;
                var testing = Context.Guild.Roles.FirstOrDefault(x => x.Name == "aafasgsagas");
                await Context.Channel.SendMessageAsync(testing.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(time + "\t" + ex);
            }
        }
        [Command("warn")]
        [RequireBotPermission(Discord.GuildPermission.KickMembers)]
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        public async Task warn(Discord.IGuildUser user, [Remainder] string reason)
        {
            if (!user.IsBot)
            {
                dbase.chkserver(Context.Guild);
                string query = "INSERT INTO `warny` (`id`, `user`, `reason`, `date`, `wanner`, `Server`) VALUES (NULL, '" + user.Id + "', @reason, '" + time.Year + "-" + time.Month + "-" + time.Day + "', '" + Context.Message.Author.Id + "', '" + dbase.Chkserver(Context.Guild.Id.ToString()) + "')";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                cmd.Parameters.AddWithValue("@reason", reason);
                dbase.make(cmd);
                Console.WriteLine(time + "\t" + user.Username + " zwarnowany za " + reason + " na serwerze " + Context.Guild.Name + "!");
                await Context.Channel.SendMessageAsync(user.Username + " zwarnowany za " + reason + "!");

                query = "SELECT COUNT(*) AS TotalNORows FROM warny as w INNER JOIN `servery` as s ON w.Server = s.id WHERE w.user='" + user.Id + "' AND s.nid='" + Context.Guild.Id.ToString() + "'";

                cmd = new MySqlCommand(query, dbase.connect);
                DataTable warn = dbase.Read(cmd);
                if (Int32.Parse(warn.Rows[0][0].ToString()) > 3)
                {
                    if (int.Parse(warn.Rows[0][0].ToString()) > 5)
                    {
                        int czas = 2 * (int.Parse(warn.Rows[0][0].ToString()) - 5);
                        string format = "yyyy-MM-dd HH:mm:ss";
                        query = "INSERT INTO `timery` (`id`, `user`, `type`, `time`, `text`, `Server`) VALUES (NULL, '" + user.Id + "', 'ban', '" + time.AddDays(czas).ToString(format) + "', NULL, '" + dbase.Chkserver(Context.Guild.Id.ToString()) + "')";
                        cmd = new MySqlCommand(query, dbase.connect);
                        dbase.timer(cmd, "ban", time.AddDays(czas), user, Context.Guild.Id.ToString());
                        Console.WriteLine(time + "\tUzytkownik " + user.Username + " zbanowany na serwerze " + Context.Guild.Name + " na " + czas + " dni.");
                        await Context.Channel.SendMessageAsync(user.Mention + " zostal zbanowany na " + czas + " dni.");
                        await user.SendMessageAsync("Zostałeś zbanowany na serwerze " + Context.Guild.Name + " na " + czas + " dni.");
                        await Context.Guild.AddBanAsync(user);
                    }
                    else
                    {
                        Console.WriteLine(time + "\tUzytkownik " + user.Username + " zkickowany na serwerze " + Context.Guild.Name + ".");
                        await Context.Channel.SendMessageAsync(user.Mention + " został zkickowany.");
                        await user.SendMessageAsync("Zostałeś zkickowany na serwerze " + Context.Guild.Name + ".");
                        await user.KickAsync();
                    }
                }
            }
            else await Context.Channel.SendMessageAsync("Nie wolno warnować botów. :(");
        }
        [Command("delwarn")]
        [RequireBotPermission(Discord.GuildPermission.KickMembers)]
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        public async Task delwarn(Discord.IGuildUser user, int warn_Num)
        {
            dbase.chkserver(Context.Guild);
            string query = "SELECT * FROM warny as w INNER JOIN `servery` as s ON w.Server = s.id WHERE w.user='" + user.Id + "' AND s.nid='" + Context.Guild.Id.ToString() + "' LIMIT " + (warn_Num - 1) + ", 1";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            if (dbase.HasRows(cmd))
            {
                query = "DELETE w.* FROM warny as w INNER JOIN (SELECT * FROM warny as w INNER JOIN `servery` as s ON w.Server = s.id WHERE w.user='" + user.Id + "' AND s.nid='" + Context.Guild.Id.ToString() + "' LIMIT " + (warn_Num - 1) + ", 1) as warny2 ON warny.id = warny2.id ";
                cmd = new MySqlCommand(query, dbase.connect);
                dbase.make(cmd);
                Console.WriteLine(time + "\t" + user.Username + " usunieto jednego warna na serwerze" + Context.Guild.Name + "!");
                await Context.Channel.SendMessageAsync("Usunieto jednego warna uzytkownikowi " + user + "!");
            }
            else await Context.Channel.SendMessageAsync("Wybrany warn nie istnieje.");
        }
        [Command("clearwarns")]
        [RequireBotPermission(Discord.GuildPermission.KickMembers)]
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        public async Task clearwarns(Discord.IGuildUser user)
        {
            dbase.chkserver(Context.Guild);
            string query = "SELECT * FROM warny as w INNER JOIN `servery` as s ON w.Server = s.id WHERE w.user='" + user.Id + "' AND s.nid='" + Context.Guild.Id.ToString() + "'";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            if (dbase.HasRows(cmd))
            {
                query = "DELETE w.* FROM warny as w INNER JOIN `servery` as s ON w.Server = s.id WHERE w.user='" + user.Id + "' AND s.nid='" + Context.Guild.Id.ToString() + "'";
                cmd = new MySqlCommand(query, dbase.connect);
                dbase.make(cmd);
                Console.WriteLine(time + "\t" + user.Username + " usunieto wszystkie warny przez " + Context.Message.Author + " Na serwerze " + Context.Guild.Name + ".");
                await Context.Channel.SendMessageAsync("Usunieto wszystkie warny uzytkownikowi " + user + " na serwerze " + Context.Guild.Name + "!");
            }
            else await Context.Channel.SendMessageAsync("Uzytkownik nie posiada warnow ~~jeszcze~~.");
        }
        [Command("listwarns")]
        public async Task listwarns(SocketUser user = null)
        {
            if (user == null) user = Context.Message.Author;
            dbase.chkserver(Context.Guild);
            string query = "SELECT  w.reason, DATE_FORMAT(w.date, '%M %e %Y'), w.wanner FROM warny as w INNER JOIN `servery` as s ON w.Server = s.id WHERE w.user='" + user.Id + "' AND s.nid='" + Context.Guild.Id.ToString() + "'";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            if (dbase.HasRows(cmd))
            {
                string a = "";
                cmd = new MySqlCommand(query, dbase.connect);
                foreach (DataRow row in dbase.Read(cmd).Rows)
                {
                    UInt64 g = Convert.ToUInt64(row[2]);
                    Discord.IGuildUser nick = Context.Guild.GetUser(g);
                    try
                    {
                        if (nick.Username != "") a += "**Data:** " + row[1] + "\t**Warnujacy:** " + nick + "\n**Powod:** " + row[0] + "\n\n";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(time + "\t Bledny warnujacy " + g + " Na serwerze " + Context.Guild.Name + ".");
                        a += "**Data:** " + row[1] + "\t**Warnujacy:** " + row[2] + "\n**Powod:** " + row[0] + "\n\n";
                    }
                }
                var b = new Discord.EmbedBuilder();
                b.WithTitle("Lista warnow");
                b.WithDescription(a);
                b.WithColor(new Discord.Color(0, 170, 255));
                await Context.Channel.SendMessageAsync("", false, b);
            }
            else
            {
                await Context.Channel.SendMessageAsync("brak warnow! :D");
            }
        }
        [Command("dmem")]
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        public async Task dMem(string nazwa = null, [Remainder]string link = null)
        {
            if (nazwa == null || link == null)
            {
                await Context.Channel.SendMessageAsync("Blad komendy poprawne uzycie ```.dmem <nazwa> <link>```");
            }
            else
            {
                dbase.chkserver(Context.Guild);
                string query = "SELECT m.text FROM memy as m INNER JOIN `servery` as s ON m.Server = s.id WHERE m.name=@name AND s.nid='" + Context.Guild.Id.ToString() + "'";

                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                cmd.Parameters.AddWithValue("@name", nazwa);

                if (!dbase.HasRows(cmd))
                {
                    query = "INSERT INTO `memy` (`id`, `name`, `text`, `Server`, `sfw`) VALUES (NULL, @name, @text, '" + dbase.Chkserver(Context.Guild.Id.ToString()) + "', 'sfw')";
                    cmd = new MySqlCommand(query, dbase.connect);
                    cmd.Parameters.AddWithValue("@name", nazwa);
                    cmd.Parameters.AddWithValue("@text", link);
                    dbase.make(cmd);
                    await Context.Channel.SendMessageAsync("Mem został dodany.");
                    Console.WriteLine(time + "\t" + "Dodano mema " + nazwa + " na serwerze " + Context.Guild.Name + ".");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Mem o podanej nazwie juz istnieje.");
                }
            }
        }
        [Command("dmemnsfw")]
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        public async Task dMemnsfw(string nazwa = null, [Remainder]string link = null)
        {
            if (nazwa == null || link == null)
            {
                await Context.Channel.SendMessageAsync("Blad komendy poprawne uzycie ```.dmem <nazwa> <link>```");
            }
            else
            {
                dbase.chkserver(Context.Guild);
                string query = "SELECT  m.text FROM memy as m INNER JOIN `servery` as s ON m.Server = s.id WHERE m.name=@name AND s.nid='" + Context.Guild.Id.ToString() + "'";

                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                cmd.Parameters.AddWithValue("@name", nazwa);

                if (!dbase.HasRows(cmd))
                {
                    query = "INSERT INTO `memy` (`id`, `name`, `text`, `Server`, `sfw`) VALUES (NULL, @name, @text, '" + dbase.Chkserver(Context.Guild.Id.ToString()) + "', 'nsfw')";
                    cmd = new MySqlCommand(query, dbase.connect);
                    cmd.Parameters.AddWithValue("@name", nazwa);
                    cmd.Parameters.AddWithValue("@text", link);
                    dbase.make(cmd);
                    await Context.Channel.SendMessageAsync("Mem został dodany.");
                    Console.WriteLine(time + "\t" + "Dodano mema " + nazwa + " na serwerze " + Context.Guild.Name + ".");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Mem o podanej nazwie juz istnieje.");
                }
            }
        }
        [Command("umem")]
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        public async Task uMem(string nazwa = null)
        {
            if (nazwa == null)
            {
                await Context.Channel.SendMessageAsync("Blad komendy poprawne uzycie ```.umem <nazwa>```");
            }
            else
            {
                dbase.chkserver(Context.Guild);
                string query = "SELECT  m.text FROM memy as m INNER JOIN `servery` as s ON m.Server = s.id WHERE m.name=@name AND s.nid='" + Context.Guild.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                cmd.Parameters.AddWithValue("@name", nazwa);
                if (dbase.HasRows(cmd))
                {
                    query = "DELETE m.* FROM `memy` as m INNER JOIN `servery` as s ON m.Server = s.id WHERE m.name=@name AND s.nid='" + Context.Guild.Id.ToString() + "'";
                    cmd = new MySqlCommand(query, dbase.connect);
                    cmd.Parameters.AddWithValue("@name", nazwa);
                    dbase.make(cmd);
                    Console.WriteLine(time + "\t" + "Usunieto mema " + nazwa + " na serwerze " + Context.Guild.Name + ".");
                    await Context.Channel.SendMessageAsync("Usunieto mema.");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Mem o podanej nazwie nie istnieje.");
                }
            }
        }
        [Command("memelist")]
        public async Task mList()
        {
            string query;
            dbase.chkserver(Context.Guild);
            if (Context.Channel.Name.Contains("lewd")) query = "SELECT m.name FROM memy as m INNER JOIN `servery` as s ON m.Server = s.id WHERE s.nid='" + Context.Guild.Id.ToString() + "'";
            else query = "SELECT m.name FROM memy as m INNER JOIN `servery` as s ON m.Server = s.id WHERE s.nid='" + Context.Guild.Id.ToString() + "' AND m.sfw = 'sfw'";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            if (dbase.HasRows(cmd))
            {

                string a = "";
                foreach (DataRow row in dbase.Read(cmd).Rows)
                {
                    a += row[0] + ", ";
                }
                a = a.Remove(a.Length - 2, 2);
                await Context.Channel.SendMessageAsync("Lista memow na tym serwerze: ```" + a + "```");
            }
            else
            {
                await Context.Channel.SendMessageAsync("brak memow na tym serwerze :P");
            }
        }
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        [Command("drank")]
        public async Task drank(string nazwa)
        {
            nazwa = nazwa.First().ToString().ToUpper() + nazwa.Substring(1);
            if (nazwa == null)
            {
                await Context.Channel.SendMessageAsync("Blad komendy poprawne uzycie ```.drank <nazwa>```");
            }
            else
            {
                dbase.chkserver(Context.Guild);
                string query = "SELECT r.name FROM ranks as r INNER JOIN `servery` as s ON r.Server = s.id WHERE r.name=@name AND s.nid='" + Context.Guild.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                cmd.Parameters.AddWithValue("@name", nazwa);
                var testing = Context.Guild.Roles.FirstOrDefault(x => x.Name == nazwa);
                if (!dbase.HasRows(cmd) && testing == null)
                {
                    query = "INSERT INTO `ranks` (`id`, `name`, `exp`, `Server`) VALUES (NULL, @name, 0, '" + dbase.Chkserver(Context.Guild.Id.ToString()) + "')";
                    cmd = new MySqlCommand(query, dbase.connect);
                    cmd.Parameters.AddWithValue("@name", nazwa);
                    dbase.make(cmd);
                    await Context.Guild.CreateRoleAsync(nazwa);
                    await Context.Channel.SendMessageAsync("Dodano range " + nazwa + ".");
                    Console.WriteLine(time + "\t" + "Dodano range " + nazwa + " na serwerze " + Context.Guild.Name + ".");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Ranga o podanej nazwie juz istnieje.");
                }
            }
        }
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        [Command("urank")]
        public async Task urank(string nazwa)
        {
            nazwa = nazwa.First().ToString().ToUpper() + nazwa.Substring(1);
            if (nazwa == null)
            {
                await Context.Channel.SendMessageAsync("Blad komendy poprawne uzycie ```.urank <nazwa>```");
            }
            else
            {
                dbase.chkserver(Context.Guild);
                string query = "SELECT  r.name FROM ranks as r INNER JOIN `servery` as s ON r.Server = s.id WHERE r.name=@name AND r.exp=0 AND s.nid='" + Context.Guild.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                cmd.Parameters.AddWithValue("@name", nazwa);
                var testing = Context.Guild.Roles.FirstOrDefault(x => x.Name == nazwa);
                if (dbase.HasRows(cmd) && testing != null)
                {
                    await testing.DeleteAsync();
                    query = "DELETE r.* FROM `ranks` as r INNER JOIN `servery` as s ON r.Server = s.id WHERE r.name=@name AND r.exp=0 AND s.nid='" + Context.Guild.Id.ToString() + "'";
                    cmd = new MySqlCommand(query, dbase.connect);
                    cmd.Parameters.AddWithValue("@name", nazwa);
                    dbase.make(cmd);
                    Console.WriteLine(time + "\t" + "Usunieto range " + nazwa + " na serwerze " + Context.Guild.Name + ".");
                    await Context.Channel.SendMessageAsync("Usunieto Range " + nazwa + ".");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Ranga o podanej nazwie nie istnieje lub nie może zostać usunięta.");
                }
            }
        }
        [Command("ranga")]
        public async Task ranga(string nazwa)
        {
            nazwa = nazwa.First().ToString().ToUpper() + nazwa.Substring(1);
            dbase.chkserver(Context.Guild);
            string query = "SELECT  r.name FROM ranks as r INNER JOIN `servery` as s ON r.Server = s.id WHERE r.name=@name AND r.exp=0 AND s.nid='" + Context.Guild.Id.ToString() + "'";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            cmd.Parameters.AddWithValue("@name", nazwa);
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == nazwa);
            if (dbase.HasRows(cmd) && role != null)
            {
                var user = Context.Message.Author as SocketGuildUser;
                if (!user.Roles.Contains(role))
                {
                    await (user as IGuildUser).AddRoleAsync(role);
                    await Context.Channel.SendMessageAsync("Nadano rangę " + nazwa + ".");
                }
                else
                {
                    await (user as IGuildUser).RemoveRoleAsync(role);
                    await Context.Channel.SendMessageAsync("Odebrano rangę " + nazwa + ".");
                }
            }
            else await Context.Channel.SendMessageAsync("Ranga o podanej nazwie nie istnieje.");
        }
        [Command("rangi")]
        public async Task rangi()
        {
            string query;
            dbase.chkserver(Context.Guild);
            query = "SELECT r.name FROM ranks as r INNER JOIN `servery` as s ON r.Server = s.id WHERE r.exp=0 AND s.nid='" + Context.Guild.Id.ToString() + "'";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            if (dbase.HasRows(cmd))
            {

                string a = "";
                foreach (DataRow row in dbase.Read(cmd).Rows)
                {
                    a += row[0] + ", ";
                }
                a = a.Remove(a.Length - 2, 2);
                await Context.Channel.SendMessageAsync("Lista rang na tym serwerze: ```" + a + "```");
            }
            else
            {
                await Context.Channel.SendMessageAsync("brak rang na tym serwerze :P");
            }
        }
        [Command("patyk")]
        public async Task patyk(SocketUser user = null)
        {
            if (user == null)
            {
                await Context.Channel.SendMessageAsync("Musisz wybrać użytkownika.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("( •_•)σ " + user.Mention);
            }
        }
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        [Command("bot")]
        public async Task bot(string type, string method = null, string ranga = null)
        {
            if (type == "init" && method == null)
            {
                string query = "SELECT * FROM servery WHERE nid='" + Context.Guild.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                if (!dbase.HasRows(cmd))
                {
                    dbase.chkserver(Context.Guild);
                    query = "INSERT INTO `adbot`(`id`, `server`, `channel`, `rank`, `enabled`) VALUES (null,'" + dbase.Chkserver(Context.Guild.Id.ToString()) + "',null ,null,false)";
                    cmd = new MySqlCommand(query, dbase.connect);
                    dbase.make(cmd);
                    await Context.Channel.SendMessageAsync("Poprawnie zainicjowano serwer.");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Twoj serwer jest juz dodany do naszego bota.");
                }
            }
            else if (type == "set")
            {
                string query = "SELECT * FROM servery WHERE nid='" + Context.Guild.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                if (dbase.HasRows(cmd))
                {
                    if (method == "channel" && ranga == null) query = "UPDATE adbot as a INNER JOIN `servery` as s ON a.server = s.id SET `channel`='" + Context.Channel.Id.ToString() + "' WHERE s.nid = '" + Context.Guild.Id.ToString() + "'";
                    else if (method == "rank" && ranga != null) query = "UPDATE adbot as a INNER JOIN `servery` as s ON a.server = s.id SET `rank`=@ranga WHERE s.nid = '" + Context.Guild.Id.ToString() + "'";
                    else
                    {
                        await Context.Channel.SendMessageAsync("niepoprawne uzycie komendy");
                        return;
                    }
                    cmd = new MySqlCommand(query, dbase.connect);
                    if (method == "rank" && ranga != null) cmd.Parameters.AddWithValue("@ranga", ranga);
                    dbase.make(cmd);
                    await Context.Channel.SendMessageAsync("Poprawne ustawienie.");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Musisz najpierw zainicjować serwer komendą ```.bot init```");
                }
            }
            else if (type == "enable")
            {
                string query = "SELECT * FROM servery WHERE nid='" + Context.Guild.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                if (dbase.HasRows(cmd))
                {
                    query = "SELECT a.* FROM adbot as a INNER JOIN servery as s ON a.server=s.id WHERE s.nid='" + Context.Guild.Id.ToString() + "'";
                    cmd = new MySqlCommand(query, dbase.connect);
                    DataTable info = dbase.Read(cmd);
                    if (info.Rows[0][2] != null && info.Rows[0][3] != null)
                    {
                        if (info.Rows[0][4].ToString() == "false")
                        {
                            query = "UPDATE adbot as a INNER JOIN `servery` as s ON a.server = s.id SET `enabled`=1 WHERE s.nid = '" + Context.Guild.Id.ToString() + "'";
                            cmd = new MySqlCommand(query, dbase.connect);
                            dbase.make(cmd);
                            await Context.Channel.SendMessageAsync("Uruchomiono adbota.");
                        }
                        else await Context.Channel.SendMessageAsync("Adbot jest już uruchomiony");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Musisz najpierw zainicjować serwer komendą ```.bot init```");
                }
            }
            else if (type == "disable")
            {
                string query = "SELECT * FROM servery WHERE nid='" + Context.Guild.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                if (dbase.HasRows(cmd))
                {
                    query = "SELECT a.* FROM adbot as a INNER JOIN servery as s ON a.server=s.id WHERE s.nid='" + Context.Guild.Id.ToString() + "'";
                    cmd = new MySqlCommand(query, dbase.connect);
                    DataTable info = dbase.Read(cmd);
                    if (info.Rows[0][2] != null && info.Rows[0][3] != null)
                    {
                        if (info.Rows[0][4].ToString() == "true")
                        {
                            query = "UPDATE adbot as a INNER JOIN `servery` as s ON a.server = s.id SET `enabled`=0 WHERE s.nid = '" + Context.Guild.Id.ToString() + "'";
                            cmd = new MySqlCommand(query, dbase.connect);
                            dbase.make(cmd);
                            await Context.Channel.SendMessageAsync("Wyłączono adbota.");
                        }
                        else await Context.Channel.SendMessageAsync("Adbot jest już wyłączony");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Musisz najpierw zainicjować serwer komendą ```.bot init```");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("Błedne użycie komendy.");
            }
        }
        [Command("rate")]
        public async Task rate(string link = null)
        {
            DateTime czas = time.AddSeconds(10);
            if (link == null) link = Context.Message.Attachments.FirstOrDefault().Url;
            var builder = new Discord.EmbedBuilder();
            builder.WithTitle("Ocena");
            builder.AddInlineField("Autor: " + Context.Message.Author.Username, "Obrazek:");
            builder.WithImageUrl(link);
            builder.WithColor(Color.DarkGreen);
            dbase.chkserver(Context.Guild);
            string query = "SELECT * FROM `servery` WHERE `nid`='" + Context.Guild.Id.ToString() + "'";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            var msg = await ((ISocketMessageChannel)Context.Guild.GetChannel(ulong.Parse(dbase.Read(cmd).Rows[0][3].ToString()))).SendMessageAsync("", false, builder);
            await msg.AddReactionAsync(Context.Guild.Emotes.First(e => e.Name == "redbean"));
            string format = "yyyy-MM-dd HH:mm:ss";
            query = "INSERT INTO `timery` (`id`, `user`, `type`, `time`, `text`, `Server`) VALUES (NULL, '" + Context.Message.Author.Id + "', 'rate', '" + czas.ToString(format) + "', '" + msg.Id + "', '" + dbase.Chkserver(Context.Guild.Id.ToString()) + "')";
            cmd = new MySqlCommand(query, dbase.connect);
            dbase.timer(cmd, "rate", czas, (IGuildUser)Context.Message.Author, Context.Guild.Id.ToString(), msg.Id.ToString());
            Console.WriteLine(time + "\tRozpoczeto glosowanie obrazka uzytkownika " + Context.Message.Author.Username + ".");
        }
        [Command("info")]
        public async Task info(SocketUser user = null)
        {
            if (user == null) user = Context.Message.Author;
            dbase.chkserver(Context.Guild);
            string query = "SELECT * FROM uzytkownicy as u INNER JOIN `servery` as s ON u.Server = s.id WHERE u.user='" + user.Id.ToString() + "' AND s.nid='" + Context.Guild.Id.ToString() + "'";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            if (!dbase.HasRows(cmd))
            {
                query = "INSERT INTO `uzytkownicy`(`id`, `user`, `score`, `ranks`, `level`, `link`, `server`) VALUES (null,'" + user.Id.ToString() + "',0 ,null ,1 ,null,  '" + dbase.Chkserver(Context.Guild.Id.ToString()) + "')";
                cmd = new MySqlCommand(query, dbase.connect);
                dbase.make(cmd);
                query = "SELECT * FROM `uzytkownicy` as u INNER JOIN `servery` as s ON u.Server = s.id WHERE u.user='" + user.Id.ToString() + "' AND s.nid='" + Context.Guild.Id.ToString() + "'";
                cmd = new MySqlCommand(query, dbase.connect);
            }
            DataTable info = dbase.Read(cmd);
            var builder = new Discord.EmbedBuilder();
            builder.WithTitle("Informacje o uzytkowniku " + user.Username);
            builder.WithThumbnailUrl(user.GetAvatarUrl());
            builder.AddField("Ilość punktów:", info.Rows[0][2]);
            builder.AddInlineField("Poziom:", info.Rows[0][4]);
            if (info.Rows[0][5] != null && info.Rows[0][5].ToString() != "") builder.AddInlineField("Deviantart:", info.Rows[0][5].ToString());
            builder.WithColor(Color.DarkGreen);
            await Context.Channel.SendMessageAsync("", false, builder);
        }
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        [Command("ratechannel")]
        public async Task info(SocketChannel channel = null)
        {
            if (channel == null)
            {
                channel = (SocketChannel)Context.Channel;
            }
            dbase.chkserver(Context.Guild);
            string query = "UPDATE `servery` SET `rate`='" + channel.Id + "' WHERE `nid`='" + Context.Guild.Id.ToString() + "'";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            dbase.make(cmd);
            await Context.Channel.SendMessageAsync("Ustawiono kanal " + channel + " jako kanal do ocen.");
        }
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        [Command("welmsg")]
        public async Task welmsg([Remainder]string msg = null)
        {
            if (msg == null)
            {
                await Context.Channel.SendMessageAsync("musisz wpisać wiadomość powitalną zastępując nazwe serwera - [server] oraz uzytkownika - [user] .");
            }
            else
            {
                dbase.chkserver(Context.Guild);
                string query = "SELECT * FROM welcome as w INNER JOIN servery as s ON w.server=s.id WHERE s.nid='" + Context.Guild.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                if (!dbase.HasRows(cmd))
                {
                    query = "INSERT INTO `welcome`(`id`, `channel`, `text`, `server`) VALUES (null,null,null,'" + dbase.Chkserver(Context.Guild.Id.ToString()) + "')";
                    cmd = new MySqlCommand(query, dbase.connect);
                    dbase.make(cmd);
                }
                query = "UPDATE welcome as w INNER JOIN servery as s ON w.server = s.id SET w.text=@text WHERE s.nid='" + Context.Guild.Id.ToString() + "'";
                cmd = new MySqlCommand(query, dbase.connect);
                cmd.Parameters.AddWithValue("@text", msg);
                dbase.make(cmd);
                await Context.Channel.SendMessageAsync("Poprawnie ustawiono wiadomość powitalną.");
            }
        }
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        [Command("welchan")]
        public async Task welchan(IGuildChannel channel = null)
        {
            if (channel == null)
                channel = (IGuildChannel)Context.Channel;
            else
            {
                dbase.chkserver(Context.Guild);
                string query = "SELECT * FROM welcome as w INNER JOIN servery as s ON w.server=s.id WHERE s.nid='" + Context.Guild.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                if (!dbase.HasRows(cmd))
                {
                    query = "INSERT INTO `welcome`(`id`, `channel`, `text`, `server`) VALUES (null,null,null,'" + dbase.Chkserver(Context.Guild.Id.ToString()) + "')";
                    cmd = new MySqlCommand(query, dbase.connect);
                    dbase.make(cmd);
                }
                query = "UPDATE welcome as w INNER JOIN servery as s ON w.server = s.id SET w.channel='" + channel.Id.ToString() + "' WHERE s.nid='" + Context.Guild.Id.ToString() + "'";
                cmd = new MySqlCommand(query, dbase.connect);
                dbase.make(cmd);
                await Context.Channel.SendMessageAsync("Poprawnie ustawiono kanał powitalny.");
            }
        }
        [Command("da")]
        public async Task da([Remainder]string link)
        {
            if (link == null || !link.Contains("deviantart.com") || (!link.Contains("https://") && !link.Contains("http://")))
            {
                await Context.Channel.SendMessageAsync("Prosze podać poprawny link do Deviantartu.");
            }
            else
            {
                dbase.chkserver(Context.Guild);
                string query = "SELECT * FROM uzytkownicy as u INNER JOIN servery as s ON u.server = s.id WHERE user = '" + Context.Message.Author.Id.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
                if (dbase.HasRows(cmd))
                {
                    query = "UPDATE uzytkownicy as u INNER JOIN servery as s ON u.server = s.id SET u.link = @link WHERE u.user = '" + Context.Message.Author.Id.ToString() + "'";
                    cmd = new MySqlCommand(query, dbase.connect);
                    cmd.Parameters.AddWithValue("@link", link);
                    dbase.make(cmd);
                }
                else
                {
                    query = "INSERT INTO `uzytkownicy`(`id`, `user`, `score`, `ranks`, `level`, `link`, `server`) VALUES(null,'" + Context.Message.Author.Id.ToString() + "',0,null,1,@link," + dbase.Chkserver(Context.Guild.Id.ToString()) + ")";
                    cmd = new MySqlCommand(query, dbase.connect);
                    cmd.Parameters.AddWithValue("@link", link);
                    dbase.make(cmd);
                }
                await Context.Channel.SendMessageAsync("Dodano twój profil deviantart. :D");
            }
        }
        [Command("pokazda")]
        public async Task MDa(SocketUser user = null)
        {
            if (user == null) user = Context.Message.Author;
            dbase.chkserver(Context.Guild);
            string query = "SELECT * FROM uzytkownicy as u INNER JOIN servery as s ON u.server = s.id WHERE u.user = '" + user.Id.ToString() + "' AND u.server = '" + dbase.Chkserver(Context.Guild.Id.ToString()) + "'";
            MySqlCommand cmd = new MySqlCommand(query, dbase.connect);
            if (dbase.HasRows(cmd))
            {
                DataTable userinfo = dbase.Read(cmd);
                if (userinfo.Rows[0][5] != null && userinfo.Rows[0][5].ToString() != "")
                    await Context.Channel.SendMessageAsync("Link do Deviantartu uzytkownika " + user.Username + ": " + userinfo.Rows[0][5].ToString() + " .");
                else
                    await Context.Channel.SendMessageAsync("Nie podano linku do da.");
            }
            else await Context.Channel.SendMessageAsync("Nie podano linku do da.");

        }
        [Command("ostatecznywkurwadme")]
        public async Task Wadme()
        {
            IGuildUser user = (IGuildUser)Context.Message.Author;
            if (user.GuildPermissions.Administrator)
            {
                var builder = new Discord.EmbedBuilder();
                builder.WithImageUrl("https://cdn.discordapp.com/attachments/349728648443592714/489137824008765462/akumucatshake.gif");
                await Context.Channel.SendMessageAsync("", false, builder);
            }
        }
    }
}

























/*[Command("rate")]
public async Task rate(string link=null)
{
    DateTime czas = time.AddSeconds(10);
    if(link==null) link = Context.Message.Attachments.FirstOrDefault().Url;

    var builder = new Discord.EmbedBuilder();

    builder.WithTitle("Ocena");
    builder.AddInlineField("Autor: " + Context.Message.Author.Username, "Obrazek:");
    builder.WithImageUrl(link);
    builder.WithColor(Color.DarkGreen);
    var msg = await ((ISocketMessageChannel)Context.Guild.GetChannel(479285934924365824)).SendMessageAsync("", false, builder);
    await msg.AddReactionAsync(Context.Guild.Emotes.First(e => e.Name == "redbean"));
    await msg.AddReactionAsync(Context.Guild.Emotes.First(e => e.Name == "yellowbean"));
    await msg.AddReactionAsync(Context.Guild.Emotes.First(e => e.Name == "greenbean"));
    await msg.AddReactionAsync(Context.Guild.Emotes.First(e => e.Name == "bluebean"));
    await msg.AddReactionAsync(Context.Guild.Emotes.First(e => e.Name == "rainbowbean"));
    string format = "yyyy-MM-dd HH:mm:ss";
    string query = "INSERT INTO `timery` (`id`, `user`, `type`, `time`, `text`, `Server`) VALUES (NULL, '" + Context.Message.Author.Id + "', 'rate', '" + czas.ToString(format) + "', '"+msg.Id+"', '" + Context.Guild.Id.ToString() + "')";
    MySqlCommand cmd = new MySqlCommand(query, instance.dbase.connect);
    instance.dbase.timer(cmd, "rate", czas, (IGuildUser)Context.Message.Author, Context.Guild.Id.ToString(), msg.Id.ToString());
    Console.WriteLine(time + "\tRozpoczeto glosowanie obrazka uzytkownika "+Context.Message.Author.Username+".");
}*/

