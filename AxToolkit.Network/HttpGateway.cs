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
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace AxToolkit.Network;

public class HttpGateway : HttpServer<HttpGatewayContext>
{
    private readonly Dictionary<string, int> _services = new Dictionary<string, int>();
    public HttpGateway(int maxConnection = 1000) : base(maxConnection) { }
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
            }
            catch (Exception ex)
            {
                return OnError(ex);
            }
        }

        request.Send(session.Service.GetStream());
        var response = HttpMessage.Read(session.Service.GetStream());
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

    private static readonly object _x509Lock = new object();
    private static X509Certificate _x509Certificate = null;

    public override X509Certificate? SelectCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate? remoteCertificate, string[] acceptableIssuers)
    {
        return null;
    }
}

public class HttpGatewayContext
{
    public TcpClient Service { get; set; }
}
