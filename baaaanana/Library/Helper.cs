using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amonikano;


namespace Amonikano.Library
{
    public static class Helper
    {
        public static bool same_user(Discord.WebSocket.SocketUser lhs, Discord.WebSocket.SocketUser rhs)
        {
            if (lhs == null)
            {
#if DEBUG
                Console.WriteLine("Tried to compare SocketUsers " + lhs.ToString() + " (= null)" + " and " + rhs.ToString() + " (= " + rhs == null ? "null)" : (rhs.ToString() + ")"));
#else
                Console.WriteLine("Tried to compare SocketUsers when at least one of them is null");
#endif
                return rhs == null;
            }
            return (lhs.Username == rhs.Username && lhs.Id == rhs.Id);
        }

        public static bool msg_mentioned_user(Discord.WebSocket.SocketUser user, Discord.WebSocket.SocketMessage msg)
        {
#if DEBUG
            bool throws_false = false;
            if (msg == null)
            {
                Console.WriteLine("Tried to find if a message mentioned a user when message is null");
                throws_false = true;
            }
            if (user == null)
            {
                Console.WriteLine("Tried to find if a message mentioned a user when user is null");
                throws_false = true;
            }
            if (throws_false)
            {
                return false;
            }
#endif
            if (msg == null || user == null)
            {
                return false;
            }
            var message_mentioned = msg.MentionedUsers;
#if DEBUG
            uint count = 0;
#endif
            foreach (Discord.WebSocket.SocketUser usr in message_mentioned)
            {
#if DEBUG
                count++;
                Console.WriteLine(count + ". " + usr.ToString());
#endif
                if (same_user(usr, user))
                {
#if DEBUG
                    Console.WriteLine("Msg mentioned " + user.ToString());
#endif
                    return true;
                }
            }
#if DEBUG
            Console.WriteLine("Msg not mentioned " + user.ToString());
#endif
            return false;
        }

        public static bool msg_mentioned_user(Discord.WebSocket.SocketUser user, Discord.WebSocket.SocketMessage msg, out string potential_command)
        {
#if DEBUG
            bool throws_false = false;
            if (msg == null)
            {
                Console.WriteLine("Tried to find if a message mentioned a user when message is null");
                throws_false = true;
            }
            if (user == null)
            {
                Console.WriteLine("Tried to find if a message mentioned a user when user is null");
                throws_false = true;
            }
            if (throws_false)
            {
                potential_command = null;
                return false;
            }
#endif
            if (msg == null || user == null)
            {
                potential_command = null;
                return false;
            }
            var message_mentioned = msg.MentionedUsers;
#if DEBUG
            uint count = 0;
#endif
            foreach (Discord.WebSocket.SocketUser usr in message_mentioned)
            {
#if DEBUG
                count++;
                Console.WriteLine(count + ". " + usr.ToString());
#endif
                if (same_user(usr, user))
                {
#if DEBUG
                    Console.WriteLine("Msg mentioned " + user.ToString());
#endif                    
                    potential_command = msg.Content;
                    if (potential_command.StartsWith(bot_const.client_mention_id))
                    {
                        potential_command = potential_command.Substring(bot_const.client_mention_id.Length);
                        potential_command = potential_command.Trim();
                    }
                    else if (potential_command.EndsWith(bot_const.client_mention_id))
                    {
                        potential_command = potential_command.Substring(0, potential_command.Length - bot_const.client_mention_id.Length);
                        potential_command = potential_command.Trim();
                    }
                    return true;
                }
            }
#if DEBUG
            Console.WriteLine("Msg not mentioned " + user.ToString());
#endif
            potential_command = null;
            return false;
        }
    }
}
