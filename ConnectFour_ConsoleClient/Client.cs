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
        public int UserId { get; set; }

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

            //server = new TcpClient("192.168.220.128", 5000);
            server = new TcpClient(localIP, 5000);
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
                message = SetUserName(Command.SetUsername);
                SendToServer(serverStream, message);

                while (playerInput != "10")
                {
                    playerInput = Console.ReadLine();

                    CreateMessage(playerInput, message);

                    SendToServer(serverStream, message);
                }
                Thread.Sleep(1000);
                server.Close();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("nu jävlar");
                Console.WriteLine(ex.Message);
            }
        }

        private void CreateMessage(string playerInput, Message message)
        {
            switch (playerInput)
            {
                case "1":
                    SetMessage(message, Command.Move, "2"); //todo 2:an är hårdkodad ska vara ett input från användaren

                    break;
                case "4":
                    Console.WriteLine("Enter new username");
                    username = Console.ReadLine();
                    SetMessage(message, Command.ChangeUserName, username);
                    break;

                case "10":
                    SetMessage(message, Command.Disconnect, "10");
                    break;

                default:
                    break;
            }
        }

        private static void SendToServer(NetworkStream serverStream, Message message)
        {
            string messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            BinaryWriter writer = new BinaryWriter(serverStream);
            writer.Write(messageJson);
            writer.Flush();
        }
        private void SetMessage(Message message, Command commandType, string messageData)
        {
            message.CommandType = commandType;
            message.MessageId = 1; // todo :
            message.MessageData = messageData;
            message.Sender = username;
            message.UserId = UserId;
        }

        private Message SetUserName(Command command)
        {
            Message message = new Message();
            username = Console.ReadLine();
            SetMessage(message, command, username);

            return message;
        }

        private void Listen()
        {
            string messageJson = "";

            try
            {
                bool running = true;
                while (running)
                {
                    NetworkStream n = server.GetStream();
                    messageJson = new BinaryReader(n).ReadString();
                    Message message = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(messageJson);
                    running = ParseMessage(running, message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private bool ParseMessage(bool running, Message message)
        {
            switch (message.CommandType)
            {
                case Command.SetUsername:
                    UserId = message.UserId;
                    Console.WriteLine(message.MessageData);
                    //Console.WriteLine($"Your username is set to {username}");
                    break;
                case Command.ChangeUserName:
                    Console.WriteLine($"Your username is changed to {username}");
                    break;
                case Command.Disconnect:
                    running = false;
                    Console.WriteLine($"You are logged out from server!");
                    break;

                default:
                    break;
            }

            return running;
        }
    }
}
