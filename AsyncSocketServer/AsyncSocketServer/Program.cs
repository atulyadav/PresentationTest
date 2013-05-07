using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Alchemy.Classes;
using Newtonsoft.Json;
using PresentationApp.DataAccess;

namespace AsyncSocketServer
{
    public class AsynchronousSocketListener
    {
        /// <summary>
        /// Store the list of online users. Wish I had a ConcurrentList.
        /// </summary>
        protected static ConcurrentDictionary<User, string> OnlineUsers = new ConcurrentDictionary<User, string>();
        static bool isHost;
        public static Guid HostGuid;
        public static Guid CurrentGuid;

        public static void Main(String[] args)
        {
            //StartListening();
            //return 0;
            //  string SslFileName = @"C:\Users\atul\Documents\presentationApp-cert.pfx";
            //   string SslPassword = @"atul";
            //    bool IsSecure = true;

            var aServer = new Alchemy.WebSocketServer(Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["serverport"].ToString()), IPAddress.Any)
            {
                OnReceive = OnReceive,
                OnConnect = OnConnect,
                OnSend = OnSend,
                OnConnected = OnConnected,
                OnDisconnect = OnDisconnect,
                //FlashAccessPolicyEnabled = true,
                // IsSecure = IsSecure,
                TimeOut = new TimeSpan(0, 10, 0)
                // SSLCertificate = new X509Certificate2(SslFileName, SslPassword)
            };
            Console.WriteLine("Server Started ");

            aServer.Start();

            // Accept commands on the console and keep it alive
            var command = string.Empty;
            while (command != "exit")
            {
                Console.WriteLine("Waiting for Host/Client to Connect...");
                command = Console.ReadLine();
            }

            aServer.Stop();

            Console.ReadLine();
        }

        public static void StartListening()
        {
        }//eof startlistening

        public static void OnConnect(UserContext context)
        {
            Console.WriteLine("Client Connection From : " + context.ClientAddress.ToString());
            //UserContexts.TryAdd(context.ClientAddress.ToString(), context);
            //context.Send("Hello from server (" + DateTime.Now.ToString() + ")");
        }

        private static void OnConnected(UserContext context)
        {
            Console.WriteLine("Client Connected From : " + context.ClientAddress.ToString());
            //add user to online list
            var me = new User { Context = context };
            UserData.createSession();
            try
            {
                Console.WriteLine("context.RequestPath = {0}", context.RequestPath);
                Guid handShakeGuid = Guid.Parse(context.RequestPath.Replace("/", " "));
                CurrentGuid = handShakeGuid;

                UserData userData = new UserData();

                if (userData.IsHost(handShakeGuid))
                {
                    if (!isHost)
                    {
                        me.IsHostServer = true;
                        isHost = true;
                        OnlineUsers.TryAdd(me, String.Empty);
                        HostGuid = handShakeGuid;
                        Console.WriteLine("this is host and it is connected now");
                    }
                }
                else if (userData.IsClient(handShakeGuid))
                {
                    me.IsHostServer = false;
                    OnlineUsers.TryAdd(me, String.Empty);
                    Console.WriteLine("this is client and it is connected now");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("in Onconnect : {0}", ex.Message);
            }
        }

        private static void OnReceive(UserContext context)
        {
            Console.WriteLine("Received Data From :" + context.ClientAddress);
            //Console.WriteLine("Data from client  : " + context.DataFrame.ToString());

            try
            {
                var json = context.DataFrame.ToString();
                Console.WriteLine("on recieve json onrecieve: {0}", json);
                dynamic obj = JsonConvert.DeserializeObject(json);

                if (obj.CommandName == "ClientStatus")
                {
                    sendUserStatus();
                }
                else if (obj.CommandName == "Chat")
                {
                    BroadcastExceptOne(JsonConvert.SerializeObject(obj), context);
                }
                else if (obj.CommandName == "RedirectToHome")
                {
                    RedirectAllClients();
                }
                else
                {
                    ChatMessage(obj, context);
                }
            }
            catch (Exception e) // Bad JSON! For shame.
            {
                Console.WriteLine("wrong : json");
                var r = new Response { Type = ResponseType.Error, Data = new { e.Message } };
                context.Send(JsonConvert.SerializeObject(r));
            }
        }

        //Send The status of connected users
        private static void sendUserStatus()
        {
            CommandData comm = new CommandData();
            UserData userData = new UserData();
            Guid uguid;
            comm.CommandName = "ClientStatus";
            foreach (var u in OnlineUsers.Keys)
            {
                uguid = Guid.Parse(u.Context.RequestPath.Replace("/", " "));
                if (u.IsHostServer == false)
                    comm.Users.Add(new UserType() { Name = (userData.GetClientName(uguid)), Type = "Participant", Guid = uguid });
                else if (u.IsHostServer == true)
                    comm.Users.Add(new UserType() { Name = (userData.GetHostName(uguid)), Type = "Presenter", Guid = uguid });
            }
            var js = JsonConvert.SerializeObject(comm);

            Broadcast(js);
        }

        private static void OnSend(UserContext context)
        {
            Console.WriteLine("Data Send To Client : {0} data = :", context.ClientAddress);
        }

        private static void OnDisconnect(UserContext context)
        {
            Console.WriteLine("Client Disconnected From : " + context.ClientAddress);
            string str;
            User user = null;

            foreach (var u in OnlineUsers.Keys)
            {
                if (u.Context.ClientAddress == context.ClientAddress)
                {
                    user = u;
                    if (u.IsHostServer == true)
                    {
                        isHost = false;
                        Console.WriteLine("Host Removed from ConcurrentDictionary : = {0} || {1}", u.Context.ClientAddress, u.Context.RequestPath);
                    }
                    else
                        Console.WriteLine("Client Removed from ConcurrentDictionary : = {0} || {1}", u.Context.ClientAddress, u.Context.RequestPath);
                    OnlineUsers.TryRemove(u, out str);
                    break;
                }
            }

            sendUserStatus();
            if (user != null)
            {
                UserData userData = new UserData();
                Guid guid = Guid.Parse(context.RequestPath.Replace("/", " "));
                userData.UpdateUserConnectionStatus(guid, user.IsHostServer);
                //if (user.IsHostServer == true)
                //{
                //    RedirectAllClients();
                //}
            }
        }

        public static void RedirectAllClients()
        {
            CommandData comm = new CommandData();
            UserData userData = new UserData();
            comm.CommandName = "RedirectToHome";
            var js = JsonConvert.SerializeObject(comm);
            Broadcast(js);
        }

        /// <summary>
        /// Broadcasts a message to all users, or if users is populated, a select list of users
        /// </summary>
        /// <param name="message">Message to be broadcast</param>
        /// <param name="users">Optional list of users to broadcast to. If null, broadcasts to all. Defaults to null.</param>
        private static void Broadcast(string message, ICollection<User> users = null)
        {
            if (users == null)
            {
                foreach (var u in OnlineUsers.Keys)
                {
                    u.Context.Send(message);
                }
            }
            else
            {
                foreach (var u in OnlineUsers.Keys.Where(users.Contains))
                {
                    Console.WriteLine("message = {0}", message);
                    u.Context.Send(message);
                }
            }
        }

        private static void BroadcastExceptOne(string message, UserContext context)
        {
            foreach (var u in OnlineUsers.Keys)
            {
                if (!(u.Context.RequestPath.Equals(context.RequestPath)))
                    u.Context.Send(message);
            }
        }

        /// <summary>
        /// Broadcasts a chat message to all online usrs
        /// </summary>
        /// <param name="message">The chat message to be broadcasted</param>
        /// <param name="context">The user's connection context</param>

        private static void ChatMessage(dynamic obj, UserContext context)
        {
            Guid handShakeGuid = Guid.Parse(context.RequestPath.Replace("/", " "));

            if (HostGuid.Equals(handShakeGuid))
            {
                Console.WriteLine("host make changes !!");
                // Broadcast(JsonConvert.SerializeObject(obj));
                BroadcastExceptOne(JsonConvert.SerializeObject(obj), context);
            }
            else
            {
                Console.WriteLine("client make changes do not broadcast !!");
                //Console.WriteLine("client user guid = " + context.RequestPath);
                messageToClient(JsonConvert.SerializeObject(obj), context);
            }
        }

        private static void messageToClient(string message, UserContext context)
        {
            context.Send(message);
        }

        public class CommandData
        {
            public string CommandName { get; set; }

            public List<UserType> Users = new List<UserType>();
        }

        /// <summary>
        /// Register a user's context for the first time with a username, and add it to the list of online users
        /// </summary>
        /// <param name="name">The name to register the user under</param>
        /// <param name="context">The user's connection context</param>
        private static void Register(string name, UserContext context)
        {
            var u = OnlineUsers.Keys.Where(o => o.Context.ClientAddress == context.ClientAddress).Single();
            var r = new Response();

            if (ValidateName(name))
            {
                u.Name = name;

                r.Type = ResponseType.Connection;
                r.Data = new { u.Name };

                Broadcast(JsonConvert.SerializeObject(r));
                OnlineUsers[u] = name;
            }
            else
            {
                SendError("Name is of incorrect length.", context);
            }
        }

        /// <summary>
        /// Broadcasts an error message to the client who caused the error
        /// </summary>
        /// <param name="errorMessage">Details of the error</param>
        /// <param name="context">The user's connection context</param>
        private static void SendError(string errorMessage, UserContext context)
        {
            var r = new Response { Type = ResponseType.Error, Data = new { Message = errorMessage } };
            context.Send(JsonConvert.SerializeObject(r));
        }

        /// <summary>
        /// Checks validity of a user's name
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <returns></returns>
        private static bool ValidateName(string name)
        {
            var isValid = false;
            if (name.Length > 3 && name.Length < 25)
            {
                isValid = true;
            }

            return isValid;
        }

        /// <summary>
        /// Defines the type of response to send back to the client for parsing logic
        /// </summary>
        public enum ResponseType
        {
            Connection = 0,
            Disconnect = 1,
            Message = 2,
            NameChange = 3,
            UserCount = 4,
            Error = 255
        }

        /// <summary>
        /// Defines the response object to send back to the client
        /// </summary>
        public class Response
        {
            public ResponseType Type { get; set; }

            public dynamic Data { get; set; }
        }

        /// <summary>
        /// Defines a type of command that the client sends to the server
        /// </summary>
        public enum CommandType
        {
            Register = 0,
            Message
        }
    }
}