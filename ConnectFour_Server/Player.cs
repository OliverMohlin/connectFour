using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConnectFour_Server
{
    class Player
    {
        private Server server;

        public static int playerCount = 1;
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
                try
                {
                    messageJson = new BinaryReader(n).ReadString();
                }
                catch (Exception)
                {
                    Console.WriteLine($"{UserName} has closed their client");
                    break;
                }

                Message message = JsonConvert.DeserializeObject<Message>(messageJson);

                running = ParseMessage(running, message);

                messageJson = JsonConvert.SerializeObject(message);

                if (message.CommandType == Command.Move)
                {
                    foreach (var player in Games.Last().Players)
                    {
                        if (player.Id != this.Id)
                        {
                            server.SendMessage(player, messageJson);
                        }
                    }
                    if (Games.Last().CheckForWinner())
                    {

                        server.Games.Remove(Games.Last());
                        Console.WriteLine(server.Games.Count);
                        Games.Remove(Games.Last());
                        Console.WriteLine(Games.Count);
                    }
                }

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
                    break;

                case Command.ChangeUserName:
                    UserName = message.MessageData;
                    Console.WriteLine($"Username of {Id} is changed to {UserName}");
                    break;

                case Command.Disconnect:
                    running = false;
                    Console.WriteLine($"{UserName} (ID: {Id}) is logged out!");
                    break;

                case Command.Move:
                    message.MessageData = JsonConvert.SerializeObject(Games.Last().PlayGame(Convert.ToInt32(message.MessageData), this));
                    if (Games.Last().CheckForWinner())
                    {
                        message.Winner = Id;
                    }

                    break;

                case Command.JoinGame:
                    if (server.Games.Count() % 2 == 0)
                    {
                        server.CreateGame();
                    }
                    message.MessageData = JsonConvert.SerializeObject(server.JoinGame(server.Games.Last().Id, this));
                    break;
                default:
                    break;
            }

            return running;
        }
    }
}
