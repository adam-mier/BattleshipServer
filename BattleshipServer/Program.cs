using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BattleshipServer
{
    class BattleshipServer
    {
        private TcpClient PlayerOne;
        private TcpClient PlayerTwo;
        private StreamReader readerP1;
        private StreamWriter writerP1;
        private StreamReader readerP2;
        private StreamWriter writerP2;
        private string PlayerTurn = "1"; //Determines Player's turn
        private int p1HitCount = 0;
        private int p2HitCount = 0;
        private string rup1 = "";
        private string rup2 = "";


        static void Main(string[] args)
        {
            new BattleshipServer();
        }

        public BattleshipServer()
        {
            Console.WriteLine("BATTLESHIP SERVER BEGINNING");
            TcpListener tcp = new TcpListener(IPAddress.Any, 1337);
            tcp.Start();

            // PLAYER 1 CONNECTION
            Console.WriteLine("WAITING FOR PLAYER 1 TO CONNECT");
            PlayerOne = tcp.AcceptTcpClient();
            readerP1 = new StreamReader(PlayerOne.GetStream());
            writerP1 = new StreamWriter(PlayerOne.GetStream());
            writerP1.WriteLineAsync("1");
            writerP1.Flush();
            Console.WriteLine("PLAYER 1 CONNECTED");

            // PLAYER 2 CONNECTION
            Console.WriteLine("WAITING FOR PLAYER 2 TO CONNECT");
            PlayerTwo = tcp.AcceptTcpClient();
            readerP2 = new StreamReader(PlayerTwo.GetStream());
            writerP2 = new StreamWriter(PlayerTwo.GetStream());
            writerP2.WriteLineAsync("2");
            writerP2.Flush();
            Console.WriteLine("PLAYER 2 CONNECTED");

            // Ready up check in their own tasks (handle out of order placing)
            Task[] tasks = new Task[2]
            {
                Task.Factory.StartNew(() => readyUpPlayerOne()),
                Task.Factory.StartNew(() => readyUpPlayerTwo())
            };

            // Wait for both players to be ready
            Task.WaitAll(tasks);

            // Begin Playing
            while (PlayerOne.Connected && PlayerTwo.Connected)
            {
                try
                {
                    // Tells each client the player's turn
                    writerP1.WriteLineAsync(PlayerTurn);
                    writerP2.WriteLineAsync(PlayerTurn);
                    writerP1.Flush();
                    writerP2.Flush();

                    if (PlayerTurn == "1")
                    {
                        var coord = readerP1.ReadLine(); //Coordinate
                        Console.WriteLine("P1: " + coord);
                        writerP2.WriteLineAsync(coord); writerP2.Flush();
                        var statusOfAtk = readerP2.ReadLine(); //Hit or Miss
                        // INCREMENT IF HIT.

                        writerP1.WriteLineAsync(statusOfAtk); writerP1.Flush();
                        writerP2.WriteLineAsync(statusOfAtk); writerP2.Flush();
                        PlayerTurn = "2";
                    }
                    else
                    {
                        var coord = readerP2.ReadLine(); //Coordinate
                        Console.WriteLine("P2: " + coord);
                        writerP1.WriteLineAsync(coord); writerP1.Flush();
                        var statusOfAtk = readerP1.ReadLine(); //Hit or Miss    
                        // INCREMENT IF HIT.
                                        
                        writerP1.WriteLineAsync(statusOfAtk); writerP1.Flush();
                        writerP2.WriteLineAsync(statusOfAtk); writerP2.Flush();
                        PlayerTurn = "1";
                    }
                }
                catch (Exception es)
                {
                    Console.WriteLine("Disconnected");
                }

            }
            Console.WriteLine("A player has disconnected.");

        }

        // TASK TO WAIT FOR PLAYER ONE TO PLACE SHIPS
        public void readyUpPlayerOne()
        {
            var readyup1 = readerP1.ReadLine();
            Console.WriteLine("Player 1 has placed their ships and is ready.");
        }

        // TASK TO WAIT FOR PLAYER TWO TO PLACE SHIPS
        public void readyUpPlayerTwo()
        {
            var readyup2 = readerP2.ReadLine();
            Console.WriteLine("Player 2 has placed their ships and is ready.");
        }
    }
}
