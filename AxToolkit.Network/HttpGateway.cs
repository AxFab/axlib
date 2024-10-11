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
using System.Globalization;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace AxToolkit.Network;

public class HttpGateway : HttpServer<HttpGatewayContext>
{
    private readonly Dictionary<string, int> _services = new Dictionary<string, int>();
    public HttpGateway() : base() { }
    public HttpGateway(int maxConnection) : base(maxConnection) { }
    public HttpGateway(Semaphore semaphore) : base(semaphore) { }

    public void AddService(string host, int port) => _services.Add(host, port);

    public override HttpMessage Handle(HttpMessage request, HttpGatewayContext session)
    {
        // Check
        if (!OnIncoming(request))
            return null;

        if (session.Service == null)
        {
            var host = request.Headers["Host"];
            try
            {
                var port = _services[host];
                session.Service = new TcpClient("localhost", port);
                session.SrvStream = session.Service.GetStream();
                // session.SrvStream = new TeeStream(session.SrvStream, $"./Logs/GW_{session.Id}_Sin.txt", $"./Logs/GW_{session.Id}_Sout.txt");
            }
            catch (Exception ex)
            {
                return OnError(ex);
            }
        }

        request.Send(session.SrvStream);
        var response = HttpMessage.Read(session.SrvStream);
        if (response == null)
            return null;
        response.KeepAlive = session.Service.Connected;
        return response;
    }

    protected virtual bool OnIncoming(HttpMessage request)
    {
        return true;
    }

    protected virtual HttpMessage? OnError(Exception ex) 
    {
        return null;
    }

    public override X509Certificate? SelectCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate? remoteCertificate, string[] acceptableIssuers)
    {
        return null;
    }
}

public class HttpGatewayContext
{
    const string KeyDigits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public HttpGatewayContext()
    {
        var str = string.Empty;
        while (str.Length < 7)
            str += KeyDigits[Random.Shared.Next(str.Length)];
        Id = str;
    }

    public TcpClient Service { get; set; }
    public Stream SrvStream { get; set; }

    public string Id { get; set; } 

}
