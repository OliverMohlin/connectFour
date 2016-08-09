using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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

        }
        public string Name { get; set; }
        public int Id { get; set; }
        public List<Game> Games { get; set; }
        public TcpClient PlayerTcp { get; set; }

        internal void Run()
        {
            string message = "";
            NetworkStream n = PlayerTcp.GetStream();

            server.SendMessage(this, "Enter Username");
            Name = new BinaryReader(n).ReadString();
            server.SendMessage(this, "Your username is:" + this.Name);

            message = new BinaryReader(n).ReadString();
            Console.WriteLine(message);
        }
    }
}
