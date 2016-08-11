using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFour_Server
{
    class Game
    {
        public Game()
        {
            Players = new List<Player>();
            Gameboard = new int[7, 7];

        }
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Player> Players { get; set; }
        public int[,] Gameboard { get; set; }

        public int[,] PlayGame(int x, Player player)
        {
            int y = Gameboard.GetLength(0) - 1;
            while (Gameboard[y, x] != 0)
            {
                y--;
            }

            Gameboard[y, x] = player.Id;

            return Gameboard;
        }


    }

}
