using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static List<Socket> clients = new List<Socket>();

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();

        Console.WriteLine("Server started on port 5000...");

        while (true)
        {
            Socket client = server.AcceptSocket();
            clients.Add(client);

            Console.WriteLine("Client connected!");

            Thread t = new Thread(() => HandleClient(client));
            t.Start();
        }
    }

    static void HandleClient(Socket client)
    {
        byte[] buffer = new byte[1024];

        while (true)
        {
            try
            {
                int bytes = client.Receive(buffer);
                if (bytes <= 0) break;

                byte[] data = new byte[bytes];
                Array.Copy(buffer, data, bytes);

                string msg = Encoding.UTF8.GetString(data);
                Console.WriteLine("Msg: " + msg);

                foreach (Socket c in clients)
                {
                    if (c != client)
                        c.Send(data);
                }
            }
            catch
            {
                break;
            }
        }
    }
}   