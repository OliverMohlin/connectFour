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
        private List<Player> Players { get; set; }
        private List<Game> Games { get; set; }
        public List<Message> MessageQueue { get; set; }

        public void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            Console.WriteLine("Seeerver is with us!!");
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
    }
}
