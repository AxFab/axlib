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
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using AxToolkit;

namespace AxToolkit.Network;

public abstract class HttpServer<TContext> : TcpServer where TContext : class
{
    protected HttpServer() : base(1000) { }
    protected HttpServer(int maxConnection) : base(maxConnection) { }
    protected HttpServer(Semaphore semaphore)
        : base(semaphore)
    {
    }

    public override void Handle(TcpClient client, bool secured)
    {
        Stream stream = client.GetStream();
        if (secured)
        {
            var certif = SelectCertificate(null, "localhost", null, null, null);
            if (certif == null)
                throw new Exception("No certificat available");
            var sslStream = new SslStream(client.GetStream(), true, ValidateCertificate);
            sslStream.AuthenticateAsServer(certif);
            stream = sslStream;
        }

        TContext context = NewSession(client, secured);
        //if (context is HttpGatewayContext gw)
        //{
        //    stream = new TeeStream(stream, $"./Logs/GW_{gw.Id}_Cin.txt", $"./Logs/GW_{gw.Id}_Cout.txt");
        //}

        bool keepConnected = true;
        while (keepConnected && client.Connected)
        {
            // Parse HTTP Request
            var request = HttpMessage.Read(stream, secured);
            if (request == null)
                return;

            // Handle request
            var response = Handle(request, context);
            if (response == null)
                return;

            // Send response
            response.Method = HttpMethod.None;
            response.Send(stream);
            keepConnected = response.KeepAlive;
        }
    }

    public abstract HttpMessage Handle(HttpMessage request, TContext session);

    public virtual TContext NewSession(TcpClient client, bool secured) => Activator.CreateInstance<TContext>();

    public virtual bool ValidateCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    public virtual X509Certificate? SelectCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate? remoteCertificate, string[] acceptableIssuers)
    {
        throw new NotImplementedException();
    }
}
