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

        public bool CheckForWinner()
        {
            //return true;
            int[,] gameBoardCopy = new int[Gameboard.GetLength(0) + 3, Gameboard.GetLength(1) + 6];
            // skapar en kopia som inte går out of bounds vid vinnarcheck
            for (int y = 0; y < Gameboard.GetLength(0); y++)
            {
                for (int x = 0; x < Gameboard.GetLength(1); x++)
                {
                    gameBoardCopy[y, x + 3] = Gameboard[y, x]; 
                }
            }

            for (int y = 0; y < gameBoardCopy.GetLength(0); y++)
            {
                for (int x = 0; x < gameBoardCopy.GetLength(1); x++)
                {
                    if (gameBoardCopy[y, x] != 0)
                    {
                        if (gameBoardCopy[y, x] == gameBoardCopy[y, x + 3] && gameBoardCopy[y, x] == gameBoardCopy[y, x + 2] && gameBoardCopy[y, x] == gameBoardCopy[y, x + 1])
                            return true;

                        if (gameBoardCopy[y, x] == gameBoardCopy[y + 3, x + 3] && gameBoardCopy[y, x] == gameBoardCopy[y + 2, x + 2] && gameBoardCopy[y, x] == gameBoardCopy[y + 1, x + 1])
                            return true;

                        if (gameBoardCopy[y, x] == gameBoardCopy[y + 3, x] && gameBoardCopy[y, x] == gameBoardCopy[y + 2, x] && gameBoardCopy[y, x] == gameBoardCopy[y + 1, x])
                            return true;

                        if (gameBoardCopy[y, x] == gameBoardCopy[y + 3, x - 3] && gameBoardCopy[y, x] == gameBoardCopy[y + 2, x - 2] && gameBoardCopy[y, x] == gameBoardCopy[y + 1, x - 1])
                            return true;
                    }
                }
            }
            return false;
        }
    }

}
