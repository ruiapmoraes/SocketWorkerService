using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketWorkerService
{
    public class SocketServerWorker : IHostedService, IDisposable
    {
        private readonly ILogger<SocketServerWorker> _logger;
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private bool _isRunning;
        private Thread _thread;

        public SocketServerWorker(ILogger<SocketServerWorker> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _isRunning = true;
            ThreadStart start = new ThreadStart(StartListening);

            _thread = new Thread(start);
            _thread.Start();

            return Task.CompletedTask;
        }

        private void StartListening()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint local = new IPEndPoint(ipAddress, 10888);

            //Criar o socket TCP/IP
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream,ProtocolType.Tcp);

            try
            {
                listener.Bind(local);
                listener.Listen(10); // max pooling 10

                _logger.LogInformation("Server started. Waiting incoming client");

                while (_isRunning)
                {
                    //Set the event to nonsignaled state
                    allDone.Reset();

                    //waitng incoming client
                    if (_isRunning)
                    {
                        listener.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            listener);
                    }
                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            _logger.LogInformation("A client was connected");


            // create a thread to serve data communication
            var t = new Thread(() => Perform(handler));
            t.Start();
        }

        private void Perform(object obj)
        {
            string data = "";
            byte[] bytes = new Byte[1024];

            Socket client = (Socket)obj;
            while (_isRunning)
            {
                int len = client.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, len);

                int index = data.IndexOf("\r\n");
                string line;
                if (index > -1)
                {
                    // print to logger
                    line = data.Substring(0, index);
                    _logger.LogInformation("Recv: {str}", line);

                    // new data
                    data = data.Substring(index + 2);

                    // exit if client sends "Exit"
                    if (line.Contains("Exit"))
                        break;
                }


            }
            client.Close();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _isRunning = false;
            allDone.Set(); // signal to main thread
            try
            {
                _thread.Join(500);
            }
            catch (Exception) { }


            return Task.CompletedTask;
        }
        public void Dispose()
        {
           
        }
    }
}
