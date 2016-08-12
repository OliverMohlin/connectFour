﻿using ConnectFour_Server;
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
        }

        private TcpClient server;
        private string username;
        public int UserId { get; set; }
        int[,] gameBoard = new int[7, 7];
        public bool MyTurn { get; set; }

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

            //server = new TcpClient("192.168.220.128", 5000);
            server = new TcpClient(localIP, 5000);
            Thread listenerThread = new Thread(Listen);
            listenerThread.Start();

            Thread senderThread = new Thread(Send);
            senderThread.Start();

            senderThread.Join();
            listenerThread.Join();

        }

        public void Send()
        {
            string playerInput = "";

            try
            {
                NetworkStream serverStream = server.GetStream();
                Message message = new Message();
                message = SetUserName(Command.SetUsername);
                SendToServer(serverStream, message);

                while (playerInput != "10")
                {
                    playerInput = GetPlayerInput("Make a choice (1-10)");
                    CreateMessage(playerInput, message);

                    if (message.CommandType != Command.Move || MyTurn)
                        SendToServer(serverStream, message);
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

        private static string GetPlayerInput(string messageToUser)
        {
            ClearMenuArea();
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
                case "1":
                    if (MyTurn)
                    {
                        bool notValidPosition = true;
                        while (notValidPosition)
                        {
                            playerInput = GetPlayerInput("Enter x-position for your next move");
                            int playerInputAsInt;
                            bool validIntInput = int.TryParse(playerInput, out playerInputAsInt);
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
                        }
                        SetMessage(message, Command.Move, playerInput);
                    }
                    else
                    {
                        Console.WriteLine("Not your turn");
                        Thread.Sleep(750);
                    }
                    break;
                case "4":
                    username = GetPlayerInput("Enter new username");
                    SetMessage(message, Command.ChangeUserName, username);
                    break;

                case "10":
                    SetMessage(message, Command.Disconnect, "10");
                    break;

                default:
                    break;
            }
        }

        private static void PrintInvalidInput()
        {
            Console.WriteLine("Invalid move, try again");
            Console.CursorTop -= 2;
        }

        private static void SendToServer(NetworkStream serverStream, Message message)
        {
            string messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(message);
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
            username = GetPlayerInput("Enter your username: ");
            SetMessage(message, command, username);

            return message;
        }

        private void Listen()
        {
            string messageJson = "";

            try
            {
                bool running = true;
                while (running)
                {
                    NetworkStream n = server.GetStream();
                    messageJson = new BinaryReader(n).ReadString();
                    Message message = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(messageJson);
                    running = ParseMessage(running, message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private bool ParseMessage(bool running, Message message)
        {
            switch (message.CommandType)
            {
                case Command.SetUsername:
                    UserId = message.UserId;
                    gameBoard = JsonConvert.DeserializeObject<int[,]>(message.MessageData);
                    Console.WriteLine($"Your username is set to {username}");
                    gameBoard = JsonConvert.DeserializeObject<int[,]>(message.MessageData);
                    DrawGameBoard(message.MessageData);
                    break;
                case Command.ChangeUserName:
                    Console.WriteLine($"Your username is changed to {username}");
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
                    break;
                default:
                    break;
            }

            return running;
        }

        private void DrawGameBoard(string messageData)
        {
            Thread.Sleep(100);
            Console.SetCursorPosition(50, 15);
            var gameBoard = JsonConvert.DeserializeObject<int[,]>(messageData);
            for (int x = 0; x < gameBoard.GetLength(0); x++)
            {
                Console.CursorLeft = 50;
                for (int y = 0; y < gameBoard.GetLength(1); y++)
                {
                    Console.Write("[" + gameBoard[x, y] + "]");
                }
                Console.WriteLine("");
            }

            Console.CursorLeft = 50;
            for (int i = 0; i < gameBoard.GetLength(0); i++)
            {
                Console.Write(" " + i + " ");
            }
            Console.SetCursorPosition(0, 7);
        }
    }
}
