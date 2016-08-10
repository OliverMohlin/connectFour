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
        public string UserName { get; set; }
        public int Id { get; set; }
        public List<Game> Games { get; set; }
        public TcpClient PlayerTcp { get; set; }

        internal void Run()
        {
            string messageJson = "";
            NetworkStream n = PlayerTcp.GetStream();

            while (true)
            {

                messageJson = new BinaryReader(n).ReadString();

                Message message = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(messageJson);

                switch (message.CommandType)
                {
                    case Command.SetUsername:
                        message.UserId = Id;
                        UserName = message.MessageData;
                        break;
                    case Command.Disconnect:
                        server.DisconnectPlayer(this);
                        PlayerTcp.Close();
                        break;
                    default:
                        break;
                }

                if (message.CommandType != Command.Disconnect)
                {
                    messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(message);

                    server.SendMessage(this, messageJson);
                }
            }
        }
    }
}
