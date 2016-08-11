using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectFour_Server
{
    class Player
    {
        private Server server;

        public static int playerCount;
        public Player(TcpClient playerTcp, Server server)
        {
            this.server = server;
            PlayerTcp = playerTcp;
            Id = playerCount++;
            Games = new List<Game>();

        }
        public string UserName { get; set; }
        public int Id { get; set; }
        public List<Game> Games { get; set; }
        public TcpClient PlayerTcp { get; set; }

        internal void Run()
        {
            string messageJson = "";
            NetworkStream n = PlayerTcp.GetStream();
            bool running = true;
            while (running)
            {

                messageJson = new BinaryReader(n).ReadString();

                Message message = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(messageJson);

                running = ParseMessage(running, message);

                messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(message);

                server.SendMessage(this, messageJson);

            }
            Thread.Sleep(1000);
            server.DisconnectPlayer(this);
            PlayerTcp.Close();
        }

        private bool ParseMessage(bool running, Message message)
        {
            switch (message.CommandType)
            {
                case Command.SetUsername:
                    message.UserId = Id;
                    UserName = message.MessageData;
                    Console.WriteLine($"Username of {Id} is set to {UserName}");
                    message.MessageData = server.JoinGame(server.Games.Last().Id, this);
                    break;

                case Command.ChangeUserName:
                    UserName = message.MessageData;
                    Console.WriteLine($"Username of {Id} is changed to {UserName}");
                    break;

                case Command.Disconnect:
                    running = false;
                    Console.WriteLine($"{UserName} ({Id}) is logged out!");
                    break;

                case Command.Move:

                    break;
                default:
                    break;
            }

            return running;
        }
    }
}
