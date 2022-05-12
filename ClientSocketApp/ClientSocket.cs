using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientSocketApp
{
    public class ClientSocket
    {
        public static void ClientSocketProducer()
        {
            Console.Write("Connecting to server........");
            //Server
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteServer = new IPEndPoint(ipAddress, 10888);

            ////Create a TCP/IP
            //Socket client = new Socket(ipAddress.AddressFamily,
            //                    SocketType.Stream, ProtocolType.Tcp);

            try
            {
                int count = 0;

                while (count < 10)
                {
                    //Create a TCP/IP
                    Socket client = new Socket(ipAddress.AddressFamily,
                                        SocketType.Stream, ProtocolType.Tcp);

                    client.Connect(remoteServer);
                    Console.WriteLine("Connected");

                    byte[] msg = Encoding.ASCII.GetBytes("This is message from client\r\n");
                    //Send the data throught the socket 
                    client.Send(msg);
                    Console.WriteLine("Sent: This is message from client");

                    msg = Encoding.ASCII.GetBytes("Second Message\r\n");
                    client.Send(msg);
                    Console.WriteLine("Sent: Second Message");

                    // send to exit
                    msg = Encoding.ASCII.GetBytes("Exit\r\n");
                    client.Send(msg);
                    Console.WriteLine("Sent: Exit");

                    // delay one second
                    Thread.Sleep(1000);

                    client.Shutdown(SocketShutdown.Both);
                    client.Close();

                    Console.WriteLine("Close socket");
                    count++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
