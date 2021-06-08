using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace client
{
    public class KeepAlive
    {

        // Properties
        private static bool Keep;
        private TcpClient Client;
        private Thread KeeperThread;

        // Events properties
        private List<IKeepAlive> Observers;

        // Methods
        public KeepAlive(TcpClient Client)
        {
            this.Client = Client;
            this.Observers = new List<IKeepAlive>();
        }

        public void Start()
        {
            Keep = true;
            KeeperThread = new Thread(new ThreadStart(Process));
            KeeperThread.Start();
        }

        public void Stop()
        {
            Keep = false;
        }

        private void Process()
        {
            Console.WriteLine("(Keeper)\tKeeper up.");

            double delay = 2000; // 4s
            DateTime lastTime = System.DateTime.Now;
            DateTime now;

            NetworkStream networkStream = Client.GetStream();

            while (Keep)
            {
                now = System.DateTime.Now;

                if ( now >= lastTime.AddMilliseconds(delay))
                {

                    try
                    {
                        // Sending message
                        byte[] sendBytes;
                        GetInput(out sendBytes);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);

                        // Receiving response
                        byte[] Bytes = new byte[Client.ReceiveBufferSize];
                        int BytesRead = networkStream.Read(Bytes, 0, (int)Client.ReceiveBufferSize);
                        GetResponse(Bytes, BytesRead);

                    }
                    catch (IOException) // Timeout
                    {
                        Console.WriteLine("(Keeper)\tServer timed out!");
                        OnServerDown();
                        break;
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("(Keeper)\tConection broken!");
                        break;
                    }

                    lastTime = now;
                }

            }
            Console.WriteLine("(Keeper)\tKeeper stopped.");
            OnKeepAliveDown();
        }

        private void GetResponse(byte[] Bytes, int BytesRead)
        {
            if (BytesRead > 0)
            {
                // Nothing
            }
            else
            {
                // Other end not responding (BytesRead == 0)
                throw new System.Net.Sockets.SocketException();
            }
        }

        private void GetInput(out byte[] Bytes)
        {
            Bytes = new byte[0];
            String Data = "keep";
            Bytes = Encoding.ASCII.GetBytes(Data);
        }

        // Interface Methods
        public void RegisterObserver(IKeepAlive observer)
        {
            Observers.Add(observer);
        }

        public void RemoveObserver(IKeepAlive observer)
        {
            Observers.Remove(observer);
        }

        // Dispachers
        public void OnServerDown()
        {
            ServerDownEvent e = new ServerDownEvent(this);
            foreach (IKeepAlive observer in Observers)
            {
                observer.OnServerDown(e);
            }
        }

        public void OnKeepAliveDown()
        {
            KeepAliveDownEvent e = new KeepAliveDownEvent(this);
            foreach (IKeepAlive observer in Observers)
            {
                observer.OnKeepAliveDown(e);
            }
        }

    }
}

