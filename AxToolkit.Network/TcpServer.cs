using System.Net;
using System.Net.Sockets;

namespace AxToolkit.Network
{
    public abstract class TcpServer
    {
        public TcpServer() : this(1000) { }
        public TcpServer(int maxConnection) : this(new Semaphore(maxConnection, maxConnection)) { }
        public TcpServer(Semaphore semaphore)
        {
            _semaphore = semaphore;
        }

        private CancellationToken _closingToken = new CancellationToken();
        private Semaphore _semaphore;

        public void Listen() => Listen(80, false);
        public void Listen(int port) => Listen(port, false);
        public void Listen(int port, bool secured)
        {
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();

            while (!_closingToken.IsCancellationRequested)
            {
                if (!_semaphore.WaitOne())
                    continue;

                var client = server.AcceptTcpClient();
                _ = Task.Run(() =>
                {
                    try
                    {
                        Handle(client, secured);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR: {ex.Message}");
                        while (ex != null)
                        {
                            Console.WriteLine($" - {ex.Message}");
                            ex = ex.InnerException;
                        } 
                        // Log!?
                    }
                    finally
                    {
                        _semaphore.Release();
                        client.Close();
                    }
                });
            }
        }

        public abstract void Handle(TcpClient client, bool secured);
    }

}
