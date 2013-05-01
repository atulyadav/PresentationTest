using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AsyncSocketServer
{
    //State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        // Handshake Done
        public bool isHandShake;
    }

    internal class TestCode
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        static private string guid = System.Configuration.ConfigurationManager.AppSettings["guid"];
        static private Int32 port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["serverport"].ToString());
        //private static readonly string _serverUrl = System.Configuration.ConfigurationManager.AppSettings["serverurl"];

        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    //listener.BeginAccept(

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            String headerResponse = "";

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            //Check if handler is not null
            if (handler != null)
            {
                Console.WriteLine("isHandShake: " + state.isHandShake);

                if (!state.isHandShake)
                {
                    string clientHandshake = String.Empty;
                    int bytesRead = handler.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        // There  might be more data, so store the data received so far.
                        state.sb.Append(Encoding.ASCII.GetString(
                            state.buffer, 0, bytesRead));

                        clientHandshake += state.sb.ToString();
                    }
                    //Last eight bytes are body of requets (we should include it in response)
                    byte[] secKey3 = state.buffer.Skip(bytesRead - 8).Take(8).ToArray();

                    //Variables we can extract from clientHandshake
                    string clientOrigin = String.Empty;
                    string secKey1 = String.Empty;
                    string secKey2 = String.Empty;
                    string WebSocketVersion = String.Empty;
                    int WSV = 0;
                    string WebSocketKey = String.Empty;

                    //Extracting values from headers (key:value)
                    string[] clientHandshakeLines = Regex.Split(clientHandshake, Environment.NewLine);
                    foreach (string hline in clientHandshakeLines)
                    {
                        int valueStartIndex = hline.IndexOf(':') + 2;
                        if (valueStartIndex > 0)
                        {
                            if (hline.StartsWith("Origin"))
                            {
                                clientOrigin = hline.Substring(valueStartIndex, hline.Length - valueStartIndex);
                            }
                            else if (hline.StartsWith("Sec-WebSocket-Key2"))
                            {
                                secKey2 = hline.Substring(valueStartIndex, hline.Length - valueStartIndex);
                            }
                            else if (hline.StartsWith("Sec-WebSocket-Key1"))
                            {
                                secKey1 = hline.Substring(valueStartIndex, hline.Length - valueStartIndex);
                            }

                            if (hline.StartsWith("Sec-WebSocket-Version"))
                            {
                                WebSocketVersion = hline.Replace("Sec-WebSocket-Version: ", "");
                                WSV = Convert.ToInt32(WebSocketVersion);
                            }

                            if (hline.StartsWith("Sec-WebSocket-Key"))
                            {
                                WebSocketKey = hline.Replace("Sec-WebSocket-Key: ", "");
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(WebSocketVersion)) //WebSocketVersion 8 and up handshake check
                    {
                        //New WebSocketVersion number, included after Version 8
                        StringBuilder mResponse = new StringBuilder();
                        mResponse.AppendLine("HTTP/1.1 101 Switching Protocols");
                        mResponse.AppendLine("Upgrade: WebSocket");
                        mResponse.AppendLine("Connection: Upgrade");
                        mResponse.AppendLine(String.Format("Sec-WebSocket-Accept: {0}", ComputeWebSocketHandshakeSecurityHash09(WebSocketKey)) + Environment.NewLine);

                        byte[] HSText = Encoding.UTF8.GetBytes(mResponse.ToString());

                        handler.Send(HSText, 0, HSText.Length, 0);

                        // Create the state object.

                        //StateObject state = new StateObject();
                        state.isHandShake = true;
                        state.workSocket = handler;

                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReadCallback), state);
                    }
                    else
                    {
                        //This part is common for all websockets editions (v. 75 & v.76)
                        handler.Send(Encoding.UTF8.GetBytes("HTTP/1.1 101 Web Socket Protocol Handshake" + Environment.NewLine));
                        handler.Send(Encoding.UTF8.GetBytes("Upgrade: WebSocket" + Environment.NewLine));
                        handler.Send(Encoding.UTF8.GetBytes("Connection: Upgrade" + Environment.NewLine));

                        if (String.IsNullOrEmpty(secKey1) && String.IsNullOrEmpty(secKey2))  //75 or less handshake check
                        {
                            handler.Send(Encoding.UTF8.GetBytes(String.Format("WebSocket-Origin: {0}", clientOrigin) + Environment.NewLine));
                            handler.Send(Encoding.UTF8.GetBytes("WebSocket-Location: " + clientOrigin.Replace("http", "ws") + "/websock" + Environment.NewLine));
                            handler.Send(Encoding.UTF8.GetBytes(Environment.NewLine));
                        }
                        else //76 handshake check
                        {
                            //Keys present, this means 76 version is used. Writing Sec-* headers
                            handler.Send(Encoding.UTF8.GetBytes(String.Format("Sec-WebSocket-Origin: {0}", clientOrigin) + Environment.NewLine));
                            handler.Send(Encoding.UTF8.GetBytes("Sec-WebSocket-Location: " + clientOrigin.Replace("http", "ws") + "/websock" + Environment.NewLine));
                            handler.Send(Encoding.UTF8.GetBytes(Environment.NewLine));

                            //Calculating response body
                            byte[] secret = CalculateSecurityBody(secKey1, secKey2, secKey3);
                            handler.Send(secret);
                        }
                    }
                }
                else
                {
                    string clientData = String.Empty;
                    int bytesRead = handler.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        // There  might be more data, so store the data received so far.
                        state.sb.Append(System.Text.Encoding.ASCII.GetString(
                            state.buffer, 0, bytesRead));

                        clientData += state.sb.ToString();
                    }
                }//fi handshake
            }//fi handler
        }//eof

        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        private static void Send(Socket handler, byte[] byteData)
        {
            // Convert the string data to byte data using ASCII encoding.
            //byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static string AcceptKey(ref string key)
        {
            string longKey = key + guid;
            SHA1 sha1 = SHA1CryptoServiceProvider.Create();
            byte[] hashBytes = sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(longKey));
            return Convert.ToBase64String(hashBytes);
        }

        private static byte[] CalculateSecurityBody(string secKey1, string secKey2, byte[] secKey3)
        {
            //Remove all symbols that are not numbers
            string k1 = Regex.Replace(secKey1, "[^0-9]", String.Empty);
            string k2 = Regex.Replace(secKey2, "[^0-9]", String.Empty);

            //Convert received string to 64 bit integer.
            Int64 intK1 = Int64.Parse(k1);
            Int64 intK2 = Int64.Parse(k2);

            //Dividing on number of spaces
            int k1Spaces = secKey1.Count(c => c == ' ');
            int k2Spaces = secKey2.Count(c => c == ' ');
            int k1FinalNum = (int)(intK1 / k1Spaces);
            int k2FinalNum = (int)(intK2 / k2Spaces);

            //Getting byte parts
            byte[] b1 = BitConverter.GetBytes(k1FinalNum).Reverse().ToArray();
            byte[] b2 = BitConverter.GetBytes(k2FinalNum).Reverse().ToArray();
            //byte[] b3 = Encoding.UTF8.GetBytes(secKey3);
            byte[] b3 = secKey3;

            //Concatenating everything into 1 byte array for hashing.
            List<byte> bChallenge = new List<byte>();
            bChallenge.AddRange(b1);
            bChallenge.AddRange(b2);
            bChallenge.AddRange(b3);

            //Hash and return
            byte[] hash = MD5.Create().ComputeHash(bChallenge.ToArray());
            return hash;
        }

        private static String ComputeWebSocketHandshakeSecurityHash09(String secWebSocketKey)
        {
            String secWebSocketAccept = String.Empty;
            // 1. Combine the request Sec-WebSocket-Key with magic key.
            String ret = secWebSocketKey + guid;
            // 2. Compute the SHA1 hash
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));
            // 3. Base64 encode the hash
            secWebSocketAccept = Convert.ToBase64String(sha1Hash);
            return secWebSocketAccept;
        }
    }
}