using System.Net;
using System.Net.Sockets;

namespace AxToolkit.Network
{
    public abstract class TcpServer
    {
        public TcpServer(int maxConnection = 1000)
        {
            _semaphore = new Semaphore(maxConnection, maxConnection);
        }
        public TcpServer(Semaphore semaphore)
        {
            _semaphore = semaphore;
        }

        private CancellationToken _closingToken = new CancellationToken();
        private Semaphore _semaphore;
        public void Listen(int port = 80, bool secured = false)
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
