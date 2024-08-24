// This file is part of AxLib.
// 
// AxLib is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software 
// Foundation, either version 3 of the License, or (at your option) any later 
// version.
// 
// AxLib is distributed in the hope that it will be useful, but WITHOUT ANY 
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
// details.
// 
// You should have received a copy of the GNU General Public License along 
// with AxLib. If not, see <https://www.gnu.org/licenses/>.
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
using System.Net;
using System.Net.Sockets;

namespace AxToolkit.Network;

public abstract class TcpServer
{
    protected TcpServer() : this(1000) { }
    protected TcpServer(int maxConnection) : this(new Semaphore(maxConnection, maxConnection)) { }
    protected TcpServer(Semaphore semaphore)
    {
        _semaphore = semaphore;
    }

    private readonly CancellationToken _closingToken = new CancellationToken();
    private readonly Semaphore _semaphore;

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
