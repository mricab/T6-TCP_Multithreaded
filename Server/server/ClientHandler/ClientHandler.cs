using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace server
{
    public class ClientHandler
    {
        //Properties
        private static bool Handle;
        private int Id;
        private TcpClient Client;
        private Thread ClientThread;

        // Events properties
        private List<IClientHandler> Observers;

        //Methods
        public ClientHandler(int Id, TcpClient Client)
        {
            this.Id = Id;
            this.Client = Client;
            this.Observers = new List<IClientHandler>();
        }

        public void Start()
        {
            Handle = true;
            ClientThread = new Thread(new ThreadStart(Process));
            ClientThread.Start();
        }

        public void Stop()
        {
            Handle = false;
            ClientThread.Join();
            //if (ClientThread != null && ClientThread.IsAlive) ClientThread.Join();
        }

        public bool Alive
        {
            get
            {
                return (ClientThread != null && ClientThread.IsAlive);
            }
        }

        private void Process()
        {
            string data = null;
            byte[] bytes; // Incoming data buffer.

            if (Client != null)
            {
                Console.WriteLine("(Handler#" + Id + ")\tStarting thread for client#" + Id + " from {0}:{1}.", ((IPEndPoint)Client.Client.RemoteEndPoint).Address, ((IPEndPoint)Client.Client.RemoteEndPoint).Port);
                Client.ReceiveTimeout = 10000; // 10s
                Client.SendTimeout = 10000; // 10s
                NetworkStream networkStream = Client.GetStream();

                while (Handle)
                {
                    bytes = new byte[Client.ReceiveBufferSize]; // 8192 Bytes
                    try
                    {
                        int BytesRead = networkStream.Read(bytes, 0, (int)Client.ReceiveBufferSize);

                        if (BytesRead > 0)
                        {
                            // Receiving
                            data = Encoding.ASCII.GetString(bytes, 0, BytesRead);
                            if (data != "keep") OnClientMessageReceived(data);
                            // Responding (Echoing)
                            byte[] sendBytes = Encoding.ASCII.GetBytes(data);
                            networkStream.Write(sendBytes, 0, sendBytes.Length);
                            // Quit
                            if (data == "quit")
                            {
                                Console.WriteLine("(Handler#" + Id + ")\tClient left.");
                                OnClientDisconnected();
                            }
                        }
                        else 
                        {
                            // Other end closed not responding (BytesRead == 0)
                            throw new System.Net.Sockets.SocketException();
                        }
                    }
                    catch (IOException)  // Timeout
                    {
                        Console.WriteLine("(Handler#" + Id + ")\tClient timed out!");
                        Handle = false;
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("(Handler#" + Id + ")\tConection broken!");
                        break;
                    }
                    Thread.Sleep(200); // 0.2s
                }

                networkStream.Close();
                Client.Close();
                Console.WriteLine("(Handler#" + Id + ")\tClient stopped.");
            }
        }

        // Interface Methods
        public void RegisterObserver(IClientHandler observer)
        {
            Observers.Add(observer);
        }

        public void RemoveObserver(IClientHandler observer)
        {
            Observers.Remove(observer);
        }

        // Dispachers
        public void OnClientMessageReceived(String message)
        {
            MessageData data = new MessageData(this.Id, message);
            MessageEvent e = new MessageEvent(this, data);
            foreach (IClientHandler observer in Observers)
            {
                observer.OnClientMessageReceived(e);
            }
        }

        public void OnClientDisconnected()
        {
            ConnectionData connection = new ConnectionData(this.Client, this.Id);
            DisconnectionEvent e = new DisconnectionEvent(this, connection);
            foreach (IClientHandler observer in Observers)
            {
                observer.OnClientDisconnected(e);
            }
        }

    }
}
