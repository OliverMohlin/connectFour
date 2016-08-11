using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectFour_Server
{
    class Server
    {
        public Server()
        {
            Games = new List<Game>();
            Players = new List<Player>();
        }
        private List<Player> Players { get; set; }
        public List<Game> Games { get; set; }
        public List<Message> MessageQueue { get; set; }

        public void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            Console.WriteLine("Seeeerver is with us!");
            Players = new List<Player>();

            try
            {
                listener.Start();

                while (true)
                {
                    TcpClient p = listener.AcceptTcpClient();
                    Player newPlayer = new Player(p, this);
                    Players.Add(newPlayer);

                    Thread clientThread = new Thread(newPlayer.Run);
                    clientThread.Start();

                    if (Games.Count() % 2 == 0)
                    {
                        CreateGame();
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
        public void SendMessage(Player player, string message)
        {
            NetworkStream clientStream = player.PlayerTcp.GetStream();
            BinaryWriter writer = new BinaryWriter(clientStream);
            writer.Write(message);
            writer.Flush();
        }

        internal void DisconnectPlayer(Player player)
        {
            Players.Remove(player);
        }

        public void CreateGame()
        {
            Game game = new Game();
            Games.Add(game);
            Console.WriteLine("A game has been created");

        }

        public int[,] JoinGame(int id, Player player)
        {

            foreach (var item in Games)
            {
                if (item.Id == id)
                {
                    if (item.Players.Count < 2)
                    {
                        item.Players.Add(player);
                        player.Games.Add(item);
                        //return $"{player.UserName} has joined game: {item.Id}";
                        return item.Gameboard;
                    }
                    //else
                        //return player.UserName + " was To slow, Go home";
                        
                }
            }
            //return "The game does not exist!";
            return new int[1,1];
        }
    }
}
