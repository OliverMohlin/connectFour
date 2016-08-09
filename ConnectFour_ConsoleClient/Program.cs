using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectFour_ConsoleClient
{
    class Program
    {

        static void Main(string[] args)
        {
            Client myClient = new Client();

            Thread clientThread = new Thread(myClient.Start);
            clientThread.Start();
            clientThread.Join();

        }
    }
}
