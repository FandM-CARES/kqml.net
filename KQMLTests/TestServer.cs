using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KQMLTests
{
    class TestServer
    {
        public TcpListener Server;
        public TcpClient Client;
        public bool ServerRunning;
        public bool ClientRunning;
        public string stuff;



        public void StartServer()
        {
            try
            {
                Server = new TcpListener(IPAddress.Parse("127.0.0.1"), 9000);
                Server.Start();
                ServerRunning = true;

                Thread t = new Thread(new ThreadStart(this.Listening));
                t.Start();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public void StartAchieveTestClient()
        {
            try
            {
                Client = new TcpClient("127.0.0.1", 8950);
                ClientRunning = true;

                Thread t = new Thread(new ThreadStart(SendAchieve));
                t.Start();
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void SendAchieve()
        {
            try
            {
                byte[] bytes = new byte[256];              

                NetworkStream stream = Client.GetStream();

                StreamWriter sw = new StreamWriter(stream);
                sw.Write("(achieve :receiver secret-agent :content (task :action (TestAchieve haha)))");

                sw.Close();

                // Shutdown and end connection
                Client.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }


        public void Listening()
        {
            try
            {
                Byte[] bytes = new Byte[256];
                String data = null;
                while (ServerRunning)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = Server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        stuff = data;

                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }

        public void Stop()
        {
            ServerRunning = false;
            Server.Stop();
        }
    }


}

