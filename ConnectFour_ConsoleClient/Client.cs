using ConnectFour_Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectFour_ConsoleClient
{
    class Client
    {
        private TcpClient server;
        private string username;

        public void Start()
        {
            #region Get local IP
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            Console.WriteLine($"IP: {localIP}");
            #endregion

            server = new TcpClient("192.168.220.128", 5000);
            //server = new TcpClient(localIP, 5000);
            Thread listenerThread = new Thread(Listen);
            listenerThread.Start();

            Thread senderThread = new Thread(Send);
            senderThread.Start();

            senderThread.Join();
            listenerThread.Join();

        }

        public void Send()
        {
            string playerInput = "";

            try
            {
                NetworkStream serverStream = server.GetStream();
                Console.WriteLine("Enter your username: ");
                Message message = new Message();
                message = SetUserName();
                SendToServer(serverStream, message);

                while (!playerInput.Equals("quit"))
                {
                    playerInput = Console.ReadLine();
                    switch (playerInput)
                    {
                        case "1":
                            message.CommandType = Command.Move;
                            message.Id = 1;
                            message.MessageData = "2";
                            message.Sender = username;
                            break;
                        case "2":

                            break;

                        default:
                            break;
                    }

                    SendToServer(serverStream, message);
                }

                server.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void SendToServer(NetworkStream serverStream, Message message)
        {
            string messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            BinaryWriter writer = new BinaryWriter(serverStream);
            writer.Write(messageJson);
            writer.Flush();
        }

        private Message SetUserName()
        {
            Message message = new Message();
            username = Console.ReadLine();
            message.CommandType = Command.SetUsername;
            message.Id = 2; // Todo : fixa det hära
            message.UserId = -1;
            message.MessageData = username;
            message.Sender = username;

            return message;
        }
        private void ChangeUserName()
        {
            // Todo : Kopia på setusername, ändra så den har userid
            Message message = new Message();
            username = Console.ReadLine();
            message.CommandType = Command.SetUsername;
            message.Id = 2;
            message.MessageData = username;
            message.Sender = username;
        }

        private void Listen()
        {
            string message = "";

            try
            {
                while (true)
                {
                    NetworkStream n = server.GetStream();
                    message = new BinaryReader(n).ReadString();
                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
