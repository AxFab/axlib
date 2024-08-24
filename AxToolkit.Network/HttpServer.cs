using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace AxToolkit.Network
{
    public abstract class HttpServer<TContext> : TcpServer where TContext : class
    {
        public HttpServer() : base(1000) { }
        public HttpServer(int maxConnection = 1000) : base(maxConnection) { }
        public HttpServer(Semaphore semaphore)
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

            bool keepConnected = true;
            TContext context = null;
            while (keepConnected && client.Connected)
            {
                // Parse HTTP Request
                var request = HttpMessage.Read(stream, secured);
                if (request == null)
                    return;

                // Handle request
                if (context == null)
                    context = NewSession(client, secured);
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

}
