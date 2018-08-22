using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Amonikano.Library.AmonikanoCommands
{

    public class CoreModule : ModuleBase
    {
        [Command("gender"), Summary(".")]
        public async Task DefineGender()
        {
            Random rng = new Random();
            int roll = rng.Next(0, 100);
            if (roll <= 49)
            {
                await (ReplyAsync("Imma boy"));
            }
            else if (roll <= 98)
            {
                await ReplyAsync("I was born a female");
            }
            else
            {
                await ReplyAsync(@"W-Why do you ask?  :flushed:");
            }
        }
        [Command("gender"), Summary("[Args].")]
        public async Task BornChildrenWithGenders
            (
            [Remainder, Summary("remainder -> args")] string args
            )
        {
            long rolls = long.Parse(args);
            Random rng = new Random();
            long female = 0, male = 0, um = 0;
            for (long i = 0; i < rolls; i++)
            {
                int roll = rng.Next(0, 100);
                if (roll <= 49)
                {
                    male++;
                }
                else if (roll <= 98)
                {
                    female++;
                }
                else
                {
                    um++;
                }
            }
            await ReplyAsync("Number of children: " + rolls + ", males: " + male + ", females: " + female);
            if (um > 0)
            {
                await ReplyAsync("There are also " + um + " other replies:");
                await ReplyAsync(@"W-Why do you ask?  :flushed:");
            }
        }
        [Command("help"), Summary("[No args] The most performant way to throw help")]
        public async Task Help_0arg()
        {
            if (!_help_initialized)
            {
                await InitializeHelp();
                _help_initialized = true;
            }
            else
            {
                await ReplyAsync(_help);
            }
        }
        [Command("help"), Summary("[Args: \n'-b': force initialize help\n] Throws help")]
        public async Task Help
            (
            [Remainder, Summary("remainder -> args")] string args
            )
        {
            if (!_help_initialized || args.Contains("-b"))
            {
                await InitializeHelp();
                _help_initialized = true;
            }
            else
            {
                await ReplyAsync(_help);
            }
        }
        [Command("check_updates"), Summary("Force the bot to check updates on github")]
        public async Task CheckUpdates()
        {
            await ReplyAsync("Checking updates");
            await ReplyAsync("Has updates: " + Updater.Program.start(null).ToString());
            await ReplyAsync("Check_update ran");

        }
        [Command("get_bytes"), Summary("Get bytes from a website")]
        public async Task GetBytes
            (
            [Remainder, Summary("remainder -> args")] string args
            )
        {
            args = args.Trim();
            using (WebClient client = new WebClient())
            {
                byte[] webBytes = client.DownloadData(args);
                string bytes_2_string = Encoding.UTF8.GetString(webBytes);
                Console.WriteLine(bytes_2_string);
                long times_to_run_minus1 = bytes_2_string.LongCount() / 1000;
                for (long i = 1; i <= times_to_run_minus1; i++)
                {
                    await ReplyAsync(bytes_2_string.Substring(((int)(i - 1) * 1000), 1000));
                }

            }
        }
        /// <summary>
        /// VERY UNOPTIMIZED
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("say"), Summary("[args: '-c | -channel' + channelID: send message to specific channel; '-d | -debug': show debug] Send message to a specific channel")]
        public async Task SendMessageToChannel
            (
            [Remainder, Summary("remainder -> args")] string args
            )
        {
            bool is_debug = false;
            Discord.WebSocket.ISocketMessageChannel channel = null;
            SocketChannel sChannel = null;
            List<string> msgList = new List<string>();
            string msg;
            string[] elements = args.Split();
            int[] msgList_index_has_mentions = new int[100];
            int index_array_count = 0;
            for (int i = 0; i < elements.Length; i++)
            {
#if TRACE
                Console.WriteLine("*_ elements["+i+"]: "+elements[i]);
#endif
                switch (elements[i])
                {
                    case "-c":
                    case "-channel":
#if TRACE
                        Console.WriteLine("+ Detected channel modification");
#endif
                        ulong channelID;
                        ulong.TryParse(elements[++i], out channelID);
                        sChannel = Program.client.GetChannel(channelID);
                        channel = (Discord.WebSocket.ISocketMessageChannel) sChannel;
#if TRACE
                        Console.WriteLine("+ Channel sending: (sChannel): " + sChannel.ToString() + ", (channel): " + channel.Name);
#endif
                        break;
                    case "-d":
                    case "-debug":
                        is_debug = true;
#if TRACE
                        Console.WriteLine("+ Will reply debug");
#endif
                        break;
                    default:
                        msgList.Add(elements[i]);
#if TRACE
                        Console.WriteLine("+ msgList new element: " + elements[i]);
#endif
                        //smart_mention
                        if (elements[i].StartsWith(bot_conf.mention_prefix))
                        {
                            msgList_index_has_mentions[index_array_count++] = msgList.Count - 1;
#if TRACE
                            Console.WriteLine("Assigned msgList_ind[" + (index_array_count - 1) + "] = " + msgList.Count);
#endif
                        }
                        break;
                }
            }
            if (index_array_count > 0&&channel != null)
            {
                //Set up dictionary
                Dictionary<string,SocketUser> lookup_table = new Dictionary<string, SocketUser>();
                SocketUser[] users = sChannel.Users.ToArray();
                for (int i = 0; i < users.Length; i++)
                {
                    string username = users[i].Username.ToLower();
#if TRACE
                    Console.WriteLine("*_+_* Username in channel " + channel.Name + ": " + username);
#endif
                    lookup_table[username] = users[i];
                    lookup_table[username + "#" + users[i].Discriminator] = users[i];
#if TRACE
                    Console.WriteLine("lookup_table[" + username + "] = (SocketUser)" + users[i].ToString());
                    Console.WriteLine("lookup_table[" + (username + "#" + users[i].Discriminator) + "] = (SocketUser)" + users[i].ToString());
#endif
                }
                //replace user's mention with correct mention
                for (int j = 0; j < index_array_count; j++)
                {
                    int index = msgList_index_has_mentions[j];
#if TRACE
                    Console.WriteLine("initial message: "+ msgList[index]);
#endif
                    string[] all_mentions = msgList[index].Split('@',StringSplitOptions.RemoveEmptyEntries);
#if TRACE
                    Console.WriteLine("all_mentions: { "+ (string.Join(", ",all_mentions))+"}");
#endif
                    for (int i = 0; i < all_mentions.Length; i++)
                    {
                        SocketUser user = lookup_table[all_mentions[i].ToLower()];
                        if (user != null)
                        {
                            all_mentions[i] = user.Mention;
                        }
                        else
                        {
                            all_mentions[i] = "@" + all_mentions[i];
                        }
                    }
                    msgList[index] = string.Join(" ", all_mentions);
#if TRACE
                    Console.WriteLine("after smart: "+msgList[index]);
#endif
                }
            }
            msg = string.Join(' ', msgList);
            if (is_debug)
            {
                if (channel == null)
                {
                    await ReplyAsync("Channel is null, meaning message will be sent on current channel");
                }
                else
                {
                    await ReplyAsync("Channel name:" + channel.Name + "; Channel ID: " + channel.Id + "; Channel is NSFW:" + channel.IsNsfw+"; Msg: "+ msg);
                }                
            }
            if (channel == null)
            {
                await ReplyAsync(msg);
            }
            else
            {
                await channel.SendMessageAsync(msg);
            }
        }
        [Command("invite_link"), Summary("Get invite website link of this bot")]
        public async Task GetInviteWebpage()
        {
            await ReplyAsync(Program.inv_webpage);
        }
#if DEBUG
        [Command("benchmark_load_modules"), Summary("Does the benchmark of the load modules")]
        public async Task BenchmarkLoadModules()
        {
            DateTime now = DateTime.Now;
            await Program.command_serv.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly());
            DateTime then = DateTime.Now;
            await ReplyAsync("Benchmark took " + (then - now).TotalMilliseconds + " ms.");
        }
#endif

        private static string mod_info_in_string(string name, string[] aliases, string summary)
        {
            return "Group " + name + " (prefix: " + aliases[0] + ", aliases:" + string.Join(", ", aliases, 1, aliases.Length - 1) + "): " + summary + "\n{";
        }
        private static string command_info_in_string(string name, string[] alias, string summary)
        {
            return " _" + name +
                (alias != null ?
                " (" + string.Join(", ", alias) + ")" : "(no alias)") + ": "
                + summary + "\n";
        }
        private static string _help = "";
        private static bool _help_initialized = false;
        public const string help_already_initialized = "help string has been initialized, and has not been changed since then";
        public async Task InitializeHelp()
        {
#if DEBUG
            DateTime now = DateTime.Now;
#endif
            CommandService service = Program.command_serv;
#if DEBUG
            DateTime then = DateTime.Now;
            await ReplyAsync("Getting service from Program.command_serv took " + (then - now).TotalMilliseconds + " ms.");
            now = DateTime.Now;
#endif
            ModuleInfo[] mods = service.Modules.ToArray<ModuleInfo>();
#if DEBUG
            then = DateTime.Now;
            await ReplyAsync("operation: var mods = service.Modules took " + (then - now).TotalMilliseconds + " ms.");
            bool got_to_foreach_loop = false;
            now = DateTime.Now;
#endif
            int count = 0;
            for (; count < mods.Length; count++)
            {
                ModuleInfo mod = mods[count];
#if DEBUG
                if (!got_to_foreach_loop)
                {
                    got_to_foreach_loop = true;
                    then = DateTime.Now;
                    await ReplyAsync("Time took to get to foreach(var mod in mods): " + (then - now).TotalMilliseconds + " ms.");
                }
                now = DateTime.Now;
#endif
                string[] aliases_array = mod.Aliases.ToArray();
#if DEBUG
                then = DateTime.Now;
                await ReplyAsync("operation: mod.Aliases.ToArray() took " + (then - now).TotalMilliseconds + " ms.");
                bool got_to_commands_foreach = false;
                now = DateTime.Now;
#endif
                string h = mod_info_in_string(mod.Name, aliases_array, mod.Summary);
                _help += h + "\n";
                await ReplyAsync(h);
                CommandInfo[] commands = mod.Commands.ToArray();
                foreach (CommandInfo command in commands)
                {
#if DEBUG
                    if (!got_to_commands_foreach)
                    {
                        got_to_commands_foreach = true;
                        then = DateTime.Now;
                        await ReplyAsync("Time took to get to foreach(var command in mod.Commands): " + (then - now).TotalMilliseconds + " ms.");
                    }
                    now = DateTime.Now;
#endif
                    string[] alias = command.Aliases.ToArray();
#if DEBUG
                    then = DateTime.Now;
                    await ReplyAsync("operation: command.Aliases.ToArray() took " + (then - now).TotalMilliseconds + " ms.");
#endif
                    h = command_info_in_string(command.Name, alias, command.Summary);
                    _help += h;
                    await ReplyAsync(h);
                }
                _help += "}";
                await ReplyAsync("}");
            }
        }
        public static void InitializeHelpNoReply()
        {
#if TRACE
            Console.WriteLine("Init help no reply called");
#endif
            _help += "What makes a message a command: Message mentions bot, message has prefix of " + bot_conf.str_prefix+"\n";
            CommandService service = Program.command_serv;
            ModuleInfo[] mods = service.Modules.ToArray<ModuleInfo>();
            int count = 0;
            int length = mods.Length;
#if TRACE
            Console.WriteLine("There are " + length + " groups");
#endif
            for (; count < length; count++)
            {
                ModuleInfo mod = mods[count];
                string[] aliases_array = mod.Aliases.ToArray();
                string h = mod_info_in_string(mod.Name, aliases_array, mod.Summary);
                _help += h + "\n";
                CommandInfo[] commands = mod.Commands.ToArray();
                foreach (CommandInfo command in commands)
                {
                    string[] alias = command.Aliases.ToArray();
                    h = command_info_in_string(command.Name, alias, command.Summary);
                    _help += h;
                }
                _help += "}\n";
                _help_initialized = true;
            }
        }
        
    }
}
