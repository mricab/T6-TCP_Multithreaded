using System;
using System.Net.Sockets;
using System.Threading;

namespace client
{
    public class Client : IServerHandler, IKeepAlive
    {
        private int ServerPort;
        private string ServerIP = "localhost";
        private int ReconnectDelay = 4000; // 4s
        private static TcpClient ClientSocket;
        private static ServerHandler Handler;
        private static KeepAlive Keeper;
        private static bool Disconnected = true;

        public Client(int ServerPort)
        {
            this.ServerPort = ServerPort;
        }

        public void Start()
        {
            try
            {
                if (AttemptConnection())
                {
                    InitilizeClient();
                    Console.WriteLine("(Client)\tClient connected.");
                    Keeper.Start();
                    Handler.Start();
                }
                else
                {
                    throw new Exception("Server not available.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("(Client)\tClient couldn't be started.");
                Console.WriteLine(e.ToString());
            }
            
        }

        public void Stop()
        {
            Keeper.Stop();
            Handler.Stop();
        }

        private void InitilizeClient()
        {
            // Initializing Main Processes
            Handler = new ServerHandler(ClientSocket);
            Keeper = new KeepAlive(ClientSocket);
            // Events Subscriptions
            Handler.RegisterObserver(this);
            Keeper.RegisterObserver(this);
        }

        private bool AttemptConnection()
        {
            uint Attempt = 1;

            while (Disconnected && Attempt <= 100)
            {
                // Initializing ClientSocket
                ClientSocket = new TcpClient();
                ClientSocket.ReceiveTimeout = 5000; // 5s
                ClientSocket.SendTimeout = 5000; // 5s

                if (ClientSocket != null)
                {
                    
                    try
                    {
                        Console.WriteLine("(Client)\tConnection attemp#{0}.", Attempt);
                        ClientSocket.Connect(ServerIP, ServerPort); //Raises error if fails
                        if (ClientSocket.Connected)
                        {
                            Disconnected = false;
                            return true;
                        }
                        
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("(Client)\tAttemp#{0} failed.", Attempt);
                        ClientSocket.Close();
                    }
                }
                else
                {
                    throw new Exception("ClientSocket is not initialized.");
                }

                ++Attempt;
                Thread.Sleep(ReconnectDelay);
            }
            return false;
            
        }

        // IServerHandler
        public void OnUserQuitted(UserQuitEvent e)
        {
            Stop();
        }

        // IKeepAlive
        public void OnServerDown(ServerDownEvent e)
        {
            Console.WriteLine("(Client)\tServer cant be reached.");
            Stop();
            Disconnected = true;
        }

        public void OnKeepAliveDown(KeepAliveDownEvent e)
        {
            if(Disconnected) Start();
        }

    }
}
