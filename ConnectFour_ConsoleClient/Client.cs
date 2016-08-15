using ConnectFour_Server;
using Newtonsoft.Json;
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
        public Client()
        {
            MyTurn = true;
            GameIsRunning = false;
            MessageQueue = new List<Message>();
            OutMessageQueue = new List<Message>();
        }
        private TcpClient server;
        private string username;
        public int UserId { get; set; }
        int[,] gameBoard = new int[7, 7];
        public bool MyTurn { get; set; }
        public bool GameIsRunning { get; set; }
        string ipAddress = "192.168.220.105";

        public List<Message> MessageQueue { get; set; }
        public List<Message> OutMessageQueue { get; set; }


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


            try
            {

                server = new TcpClient(ipAddress, 5000);
                //server = new TcpClient(localIP, 5000);

                Thread listenerThread = new Thread(Listen);
                listenerThread.Start();

                Thread senderThread = new Thread(Send);
                senderThread.Start();

                bool running = true;
                while (running)
                {
                    NetworkStream serverStream = server.GetStream();
                    if (OutMessageQueue.Count > 0)
                    {
                        SendToServer(serverStream, OutMessageQueue.First());
                        OutMessageQueue.Remove(OutMessageQueue.First());
                    }

                }

                senderThread.Join();
                listenerThread.Join();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Local IP: {localIP}");
                Console.WriteLine($"Server IP: {ipAddress}");

                Console.Write("You have no connection to a server, please enter a new IP address: ");
                ipAddress = Console.ReadLine();

                Start();

                Console.WriteLine(ex);
            }


        }

        public void Send()
        {
            string playerInput = "";

            try
            {
                Message message = new Message();
                message = SetUserName(Command.SetUsername);
                OutMessageQueue.Add(message);
                bool running = true;
                while (running)
                {

                    if (MessageQueue.Count > 0)
                    {
                        running = ParseMessage(running, MessageQueue.First());
                        MessageQueue.Remove(MessageQueue.First());
                    }

                }

                while (playerInput != "10")
                {

                    if (message.CommandType != Command.Move && message.CommandType != Command.JoinGame || !GameIsRunning)
                    {
                        bool notValidInput = true;
                        bool notANumber = true;
                        while (notValidInput || notANumber)
                        {
                            ClearMenuArea(); //
                            HandleMenu(); //
                            playerInput = GetPlayerInput("");
                            int playerInputAsInt = 0;
                            notANumber = !(int.TryParse(playerInput, out playerInputAsInt));
                            if (Enum.IsDefined(typeof(Command), playerInputAsInt))
                            {
                                notValidInput = false;
                            }
                            else
                            {
                                Console.WriteLine("Incorrect input");
                                Thread.Sleep(500);
                            }
                        }
                    }
                    else
                        playerInput = "1";

                    CreateMessage(playerInput, message); // Spellogik

                    //SendToServer(serverStream, message);
                    if (message.CommandType == Command.Move && MyTurn)
                        MyTurn = false;
                }
                Thread.Sleep(1000);
                server.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void Listen()
        {
            string messageJson = "";

            try
            {
                while (true) //Todo: :)
                {
                    NetworkStream n = server.GetStream();
                    messageJson = new BinaryReader(n).ReadString();
                    Message message = JsonConvert.DeserializeObject<Message>(messageJson);
                    MessageQueue.Add(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string GetPlayerInput(string messageToUser)
        {
            Console.WriteLine(messageToUser);
            string playerInput = "";
            bool notValidInput = true;
            while (notValidInput)
            {
                playerInput = Console.ReadLine();
                if (playerInput != null && playerInput != "")
                    notValidInput = false;
            }

            return playerInput;
        }

        private static void ClearMenuArea()
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    Console.SetCursorPosition(j, i);
                    Console.Write(" ");
                }

            }
            Console.SetCursorPosition(0, 0);
        }

        private void CreateMessage(string playerInput, Message message)
        {
            switch (playerInput)
            {
                case "4":
                    ClearMenuArea();
                    username = GetPlayerInput("Enter new username");
                    SetMessage(message, Command.ChangeUserName, username);
                    break;

                case "5":
                    GameIsRunning = true;
                    SetMessage(message, Command.JoinGame, username);
                    MyTurn = true;
                    break;

                case "10":
                    SetMessage(message, Command.Disconnect, "10");
                    break;

                default:
                    Console.WriteLine("Invalid input");
                    break;

            }
        }

        private static void PrintInvalidInput()
        {
            Console.WriteLine("Invalid move, try again");
            Console.CursorTop -= 2;
            Thread.Sleep(500);
        }
        private static void SendToServer(NetworkStream serverStream, Message message)
        {
            string messageJson = JsonConvert.SerializeObject(message);
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
            ClearMenuArea();
            username = GetPlayerInput("Enter your username: ");
            SetMessage(message, command, username);

            return message;
        }
        private bool ParseMessage(bool running, Message message)
        {
            switch (message.CommandType)
            {
                case Command.SetUsername:
                    UserId = message.UserId;
                    Console.WriteLine($"Your username is set to {username}");
                    HandleMenu();
                    break;
                case Command.ChangeUserName:
                    Console.WriteLine($"Your username is changed to {username}");
                    HandleMenu();
                    break;
                case Command.Disconnect:
                    running = false;
                    Console.WriteLine($"You are logged out from server!");
                    break;
                case Command.Move:
                    gameBoard = JsonConvert.DeserializeObject<int[,]>(message.MessageData);
                    DrawGameBoard(message.MessageData);

                    if (message.UserId != UserId)
                    {
                        MyTurn = true;
                    }
                    else
                        MyTurn = false;

                    if (message.Winner != 0)
                    {
                        GameIsRunning = false;
                    }

                    if (GameIsRunning)
                    {
                        if (!MyTurn)
                        {
                            Thread.Sleep(50);
                            Console.Write("Waiting for other player");
                        }

                        if (MyTurn)
                        {
                            string input;
                            bool notValidPosition = true;
                            while (notValidPosition)
                            {
                                ClearMenuArea();
                                input = GetPlayerInput("Enter x-position for your next move");
                                int playerInputAsInt;
                                bool validIntInput = int.TryParse(input, out playerInputAsInt);
                                if (validIntInput)
                                {
                                    if (playerInputAsInt >= 0 && playerInputAsInt < gameBoard.GetLength(1))
                                    {
                                        if (gameBoard[0, playerInputAsInt] == 0)
                                        {
                                            notValidPosition = false;
                                        }
                                        else
                                        {
                                            PrintInvalidInput();
                                        }
                                    }
                                    else
                                    {
                                        PrintInvalidInput();
                                    }
                                }
                                else
                                {
                                    PrintInvalidInput();
                                }
                                SetMessage(message, Command.Move, input);
                                OutMessageQueue.Add(message);
                            }
                        }
                    }
                    else
                    {

                        if (message.Winner == UserId)
                        {
                            Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.CursorTop);
                            Console.WriteLine("YOU WON!");

                        }
                        else
                        {
                            Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.CursorTop);
                            Console.WriteLine("YOU LOST!");
                        }
                        string endText = "Press the 'any' key to continue...";
                        Console.SetCursorPosition(Console.WindowWidth / 2 - (endText.Length / 2), Console.WindowHeight - 1);
                        Console.Write(endText);
                        Console.ReadKey();
                        Console.Clear();

                        HandleMenu();
                    }

                    break;

                case Command.JoinGame:
                    gameBoard = JsonConvert.DeserializeObject<int[,]>(message.MessageData);
                    Console.Clear();
                    DrawGameBoard(message.MessageData);
                    string playerInput = GetPlayerInput("Enter x-position for your next move:");
                    SetMessage(message, Command.Move, playerInput);
                    OutMessageQueue.Add(message);
                    break;

                default:
                    break;
            }
            return running;
        }

        private void HandleMenu()
        {
            Thread.Sleep(500);
            Console.WriteLine("4. Change Username");
            Console.WriteLine("5. Join Game");
            string playerInput = Console.ReadLine();
            Message message = new Message();
            CreateMessage(playerInput, message);
            OutMessageQueue.Add(message);
        }

        private void DrawGameBoard(string messageData)
        {
            Thread.Sleep(500);
            Console.Clear();
            Console.WriteLine("============CONNECT FOUR============");
            Console.SetCursorPosition(50, 15);
            var gameBoard = JsonConvert.DeserializeObject<int[,]>(messageData);
            ConsoleColor tempForegroundColor;
            ConsoleColor tempBackgroundColor;
            for (int x = 0; x < gameBoard.GetLength(0); x++)
            {
                tempBackgroundColor = Console.BackgroundColor;
                Console.BackgroundColor = ConsoleColor.White;
                Console.CursorLeft = 50;
                for (int y = 0; y < gameBoard.GetLength(1); y++)
                {
                    if (gameBoard[x, y] == 0)
                        Console.Write("   ");

                    else if (gameBoard[x, y] == UserId)
                    {
                        tempForegroundColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("[x]");
                        Console.ForegroundColor = tempForegroundColor;
                    }
                    else
                    {
                        tempForegroundColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("[o]");
                        Console.ForegroundColor = tempForegroundColor;
                    }
                }
                Console.BackgroundColor = tempBackgroundColor;

                Console.WriteLine("");
            }

            Console.CursorLeft = 50;
            for (int i = 0; i < gameBoard.GetLength(0); i++)
            {
                Console.Write(" " + i + " ");
            }
            Console.SetCursorPosition(0, 2);
        }
    }
}
