using System.Threading.Tasks;
using System;
using System.Data;
using System.Timers;
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
using Newtonsoft.Json;

namespace bot
{
    public class db
    {
        private string connection;
        private string data;
        private Config config = new Config();
        public MySqlConnection connect;
        public MySqlDataReader read;
        public DiscordSocketClient client = new DiscordSocketClient();
        System.DateTime time = System.DateTime.Now;
        public db()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                data = sr.ReadToEnd();
            }
            config = JsonConvert.DeserializeObject<Config>(data);
            connection = config.MySQL;
            client.LoginAsync(TokenType.Bot, config.BotKey);
            client.StartAsync();
            connect = new MySqlConnection(connection);
        }
        public bool start()
        {
            try
            {
                string query = "SELECT * FROM `timery` WHERE 1";
                MySqlCommand cmd = new MySqlCommand(query, connect);
                DataTable timery = Read(cmd);
                if (HasRows(cmd))
                {
                    query = "SELECT COUNT(*) AS TotalNORows FROM timery WHERE 1";
                    cmd = new MySqlCommand(query, connect);
                    Timer[] count = new Timer[Int32.Parse(Read(cmd).Rows[0][0].ToString())];
                    int i = 0;
                    foreach (DataRow row in timery.Rows)
                    {
                        time = System.DateTime.Now;
                        if (DateTime.Parse(row[3].ToString()).CompareTo(time) > 0)
                        {
                            count[i] = new Timer();
                            if (row[2].ToString() == "ban") count[i].Elapsed += (sender, e) => onTime(sender, e, row[2].ToString(), row[1].ToString(), row[5].ToString());
                            else count[i].Elapsed += (sender, e) => onTime(sender, e, row[2].ToString(), row[1].ToString(), row[5].ToString(), row[4].ToString());
                            count[i].AutoReset = false;
                            count[i].Interval = (DateTime.Parse(row[3].ToString()) - time).TotalSeconds * 1000;
                            count[i].Enabled = true;
                            i++;
                        }
                        else
                        {
                            count[i] = new Timer();
                            if (row[2].ToString() == "ban") count[i].Elapsed += (sender, e) => onTime(sender, e, row[2].ToString(), row[1].ToString(), row[5].ToString());
                            else count[i].Elapsed += (sender, e) => onTime(sender, e, row[2].ToString(), row[1].ToString(), row[5].ToString(), row[4].ToString());
                            count[i].AutoReset = false;
                            count[i].Interval = 1;
                            count[i].Enabled = true;
                        }
                    }
                }
                Console.WriteLine(time + "\tPoprawnie polaczono z baza danych.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(time + "\tBlad polaczenia bazy danych.");
                Console.WriteLine(time + "\t" + ex.Message);
                if (connect.State != System.Data.ConnectionState.Closed) connect.Close();
                return false;
            }
        }
        public bool HasRows(MySqlCommand cmd)
        {
            try
            {
                connect.Open();
                read = cmd.ExecuteReader();
                if (read.HasRows)
                {
                    connect.Close();
                    return true;
                }
                else
                {
                    connect.Close();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(time + "\t" + ex);
                if (connect.State != System.Data.ConnectionState.Closed) connect.Close();
                return false;
            }
        }
        public void make(MySqlCommand cmd)
        {
            try
            {
                connect.Open();
                read = cmd.ExecuteReader();
                connect.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(time + "\t" + ex);
                if (connect.State != System.Data.ConnectionState.Closed) connect.Close();
                return;
            }
        }
        public DataTable Read(MySqlCommand cmd)
        {
            DataTable results = new DataTable();
            try
            {
                connect.Open();
                read = cmd.ExecuteReader();
                results.Load(read);
                connect.Close();
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(time + "\t" + ex);
                if (connect.State != System.Data.ConnectionState.Closed) connect.Close();
                return null;
            }
        }
        public void timer(MySqlCommand cmd, string type, DateTime timing, Discord.IGuildUser user, string server, string text = null)
        {
            try
            {
                connect.Open();
                read = cmd.ExecuteReader();
                connect.Close();
                if (timing.CompareTo(time) > 0)
                {
                    Timer count = new Timer();
                    if (type == "rate") count.Elapsed += (sender, e) => onTime(sender, e, type, user.Id.ToString(), server, text);
                    else count.Elapsed += (sender, e) => onTime(sender, e, type, user.Id.ToString(), server);
                    count.AutoReset = false;
                    count.Interval = (timing - time).TotalSeconds * 1000;
                    count.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(time + "\t" + ex);
                if (connect.State != System.Data.ConnectionState.Closed) connect.Close();
                return;
            }
        }
        public async void onTime(object source, System.Timers.ElapsedEventArgs e, string type, string user, string server, string text = null)
        {

            time = System.DateTime.Now;
            if (type == "ban")
            {
                try
                {
                    string format = "yyyy-MM-dd HH:mm:ss";
                    string query = "DELETE t.* FROM timery as t INNER JOIN `servery` as s ON t.Server = s.id WHERE t.user='" + user + "' AND s.nid='" + server + "' AND time<='" + time.ToString(format) + "'";
                    MySqlCommand cmd = new MySqlCommand(query, connect);
                    make(cmd);
                    Console.WriteLine(time + "\tOdbanowano uzytkownika " + client.GetUser(ulong.Parse(user)).Username + " na serwerze " + client.GetGuild(ulong.Parse(server)).Name + ".");
                    await client.GetGuild(ulong.Parse(server)).RemoveBanAsync(ulong.Parse(user));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(time + "\t" + ex);
                }
            }
            else if (type == "rate")
            {
                try
                {
                    string format = "yyyy-MM-dd HH:mm:ss";
                    string query = "SELECT * FROM timery WHERE user='" + user + "' AND type='" + type + "' AND time<='" + time.ToString(format) + "'";
                    MySqlCommand cmd = new MySqlCommand(query, connect);
                    DataTable rating = Read(cmd);
                    query = "DELETE t.* FROM timery as t INNER JOIN `servery` as s ON t.Server = s.id WHERE t.user='" + user + "' AND s.nid='" + server + "' AND t.time<='" + time.ToString(format) + "'";
                    cmd = new MySqlCommand(query, connect);
                    make(cmd);
                    Console.WriteLine(time + "\tZakonczono glosowanie na obrazek uzytkownika " + client.GetUser(ulong.Parse(user)).Username + ".");
                    var guild = client.GetGuild(ulong.Parse(server));
                    query = "SELECT * FROM `servery` WHERE `nid`='" + server + "'";
                    cmd = new MySqlCommand(query, connect);
                    var channel = guild.GetTextChannel(ulong.Parse(Read(cmd).Rows[0][3].ToString()));
                    var msg = (RestUserMessage)(await channel.GetMessageAsync(ulong.Parse(text)));
                    int wynik = 0;
                    var grades = await msg.GetReactionUsersAsync(":redbean:479295226574274570");
                    string[,] users = new string[grades.Count(), 2];
                    for (int i = 0; i < grades.Count(); i++)
                    {
                        if (grades.ElementAt(i).IsBot || grades.ElementAt(i).Id.ToString() == user)
                        {
                            break;
                        }
                        else
                        {
                            wynik += 1;
                        }
                    }
                    query = "SELECT u.* FROM `uzytkownicy` as u INNER JOIN `servery` as s ON u.Server = s.id WHERE u.user='" + user + "' AND s.nid='" + server + "'";
                    cmd = new MySqlCommand(query, connect);
                    DataTable info = Read(cmd);
                    if (HasRows(cmd))
                    {
                        query = "UPDATE `uzytkownicy` SET `score`='" + (Int32.Parse(info.Rows[0][2].ToString()) + wynik) + "' WHERE user='" + user + "' AND server='" + Chkserver(server) + "'";
                        cmd = new MySqlCommand(query, connect);
                        make(cmd);
                    }
                    else
                    {
                        query = "INSERT INTO `uzytkownicy`(`id`, `user`, `score`, `ranks`, `level`, `link`, `server`) VALUES (null,'" + user + "','" + wynik + "',null ,1 ,null,  '" + Chkserver(server) + "')";
                        cmd = new MySqlCommand(query, connect);
                        make(cmd);
                    }
                    await msg.RemoveAllReactionsAsync();
                    await channel.SendMessageAsync("Zakończono ocene obrazka użytkownika " + client.GetUser(ulong.Parse(user)).Mention + " wynik: " + wynik + ".");
                    ExpUser((SocketUser)msg.Author, guild);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(time + "\t" + ex);
                }
            }
        }
        public void ExpUser(SocketUser user, SocketGuild guild)
        {
            string query = "SELECT u.* FROM uzytkownicy as u INNER JOIN `servery` as s ON u.Server = s.id WHERE s.nid='" + guild.Id.ToString() + "' AND u.user='" + user.Id.ToString() + "'";
            MySqlCommand cmd = new MySqlCommand(query, connect);
            DataTable info = Read(cmd);
            int level;
            string rank;
        }
        public int Chkserver(string nid)
        {
            string query = "SELECT * FROM `servery` WHERE `nid`='" + nid + "'";
            MySqlCommand cmd = new MySqlCommand(query, connect);
            return Int32.Parse(Read(cmd).Rows[0][0].ToString());
        }
        public void chkserver(SocketGuild guild)
        {
            string query = "SELECT * FROM `servery` WHERE `nid`='" + guild.Id.ToString() + "'";
            MySqlCommand cmd = new MySqlCommand(query, connect);
            if (!HasRows(cmd))
            {
                query = "INSERT INTO `servery`(`id`, `name`, `nid`, `rate`) VALUES (null, '" + guild.Name + "','" + guild.Id.ToString() + "',null)";
                cmd = new MySqlCommand(query, connect);
                make(cmd);
            }
        }
    }
}










/*try
                {
                    string format = "yyyy-MM-dd HH:mm:ss";
                    string query = "SELECT * FROM `timery` WHERE user='" + user + "' AND type='" + type + "' AND time<='" + time.ToString(format) + "'";
                    MySqlCommand cmd = new MySqlCommand(query, connect);
                    DataTable rating = Read(cmd);
                    query = "DELETE FROM `timery` WHERE user='" + user + "' AND Server='" + server + "' AND time<='" + time.ToString(format) + "'";
                    cmd = new MySqlCommand(query, connect);
                    make(cmd);
                    Console.WriteLine(time + "\tZakonczono glosowanie na obrazek uzytkownika " + client.GetUser(ulong.Parse(user)).Username + ".");
                    var guild = client.GetGuild(ulong.Parse(server));
                    var channel = guild.GetTextChannel(479285934924365824);
                    var msg = (RestUserMessage)(await channel.GetMessageAsync(ulong.Parse(text)));
                    int wynik = 0;
                    int lp = 0;
                    var ybean = await msg.GetReactionUsersAsync(":yellowbean:479310275967713301");
                    var rbean = await msg.GetReactionUsersAsync(":redbean:479310257894588433");
                    var gbean = await msg.GetReactionUsersAsync(":greenbean:479310238412046336");
                    var bbean = await msg.GetReactionUsersAsync(":bluebean:479310225820876800");
                    var rabean = await msg.GetReactionUsersAsync(":rainbowbean:479310288697557004");
                    var everyone = ybean.Concat(rbean).Concat(gbean).Concat(bbean).Concat(rabean);
                    string[,] users = new string[everyone.Count(), 3];
                    for (int i=0; i < everyone.Count(); i++)
                    {
                        for(int j=0; j < users.GetLength(0); j++)
                        {
                            if (lp - j == 0 && !everyone.ElementAt(i).IsBot)
                            {
                                users[j, 0] = everyone.ElementAt(i).Id.ToString();
                                users[j, 1] = "1";
                                users[j, 2] = "5";
                                wynik++;
                                lp++;
                                break;
                            }
                            else if ((everyone.ElementAt(i).Id.ToString() == users[j, 0] && users[j, 1]==users[j, 2]) || everyone.ElementAt(i).IsBot)
                            {
                                break;
                            }
                            else if(everyone.ElementAt(i).Id.ToString()==users[j, 0])
                            {
                                users[j, 1] = (Int32.Parse(users[j, 1])+1).ToString();
                                wynik++;
                                break;
                            }
                        }
                    }

                    query = "SELECT * FROM `artysci` WHERE user='" + user + "' AND server='" + server + "'";
                    cmd = new MySqlCommand(query, connect);
                    DataTable info = Read(cmd);
                    if (HasRows(cmd))
                    {
                        if(Int32.Parse(info.Rows[0][5].ToString()) + lp == 0) query = "UPDATE `artysci` SET `score`='"+(Int32.Parse(info.Rows[0][2].ToString())+wynik)+"',`average`='"+ Math.Round((double.Parse(info.Rows[0][2].ToString())+wynik)/(Int32.Parse(info.Rows[0][5].ToString())+lp+1), 1) + "', `ilosc glosow`='" + (Int32.Parse(info.Rows[0][5].ToString()) + lp) + "'  WHERE user='" + user + "' AND server='" + server + "'";
                        else query = "UPDATE `artysci` SET `score`='" + (Int32.Parse(info.Rows[0][2].ToString()) + wynik) + "',`average`='" + Math.Round((double.Parse(info.Rows[0][2].ToString()) + wynik) / (Int32.Parse(info.Rows[0][5].ToString()) + lp), 1) + "', `ilosc glosow`='"+ (Int32.Parse(info.Rows[0][5].ToString()) + lp) + "' WHERE user='" + user + "' AND server='" + server + "'";
                        cmd = new MySqlCommand(query, connect);
                        make(cmd);
                    }
                    else
                    {
                        if(lp==0) query = "INSERT INTO `artysci`(`id`, `user`, `score`, `average`, `server`, `ilosc glosow`) VALUES (null,'" + user + "','" + wynik + "','" + Math.Round((double.Parse(wynik.ToString()) / (lp + 1)), 1) + "','" + server + "', '"+lp+"')";
                        else query = "INSERT INTO `artysci`(`id`, `user`, `score`, `average`, `server`, `ilosc glosow`) VALUES (null,'" + user + "','" + wynik + "','" + Math.Round((double.Parse(wynik.ToString()) / (lp)), 1) + "','" + server + "', '" + lp + "')";
                        cmd = new MySqlCommand(query, connect);
                        make(cmd);
                    }
                    if (lp == 0)
                    {
                        lp = 1;
                    }
                    await msg.RemoveAllReactionsAsync();
                    await channel.SendMessageAsync("Zakończono ocene obrazka użytkownika " + client.GetUser(ulong.Parse(user)).Mention + " wynik: " + wynik + " średnia ocen: "+(double.Parse(wynik.ToString())/lp)+".");
                }catch(Exception ex)
                {
                    Console.WriteLine(time +"\t"+ ex);
                }
*/
