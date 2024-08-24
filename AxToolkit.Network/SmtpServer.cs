using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace AxToolkit.Network
{
    public class SmtpServer : TcpServer
    {
        public string Hostname { get; private set; } = "axfab.net";
        public int MaxMessageSize { get; private set; } = 1024 * 12024 * 5;
        public int MaxBadCommands { get; private set; } = 5;
        public int MaxRecipients { get; private set; } = 50;

        public override void Handle(TcpClient client, bool secured)
        {
            var stream = client.GetStream();
            var session = new SmtpSession(this, stream, stream);
            session.Remote = client.Client.RemoteEndPoint?.ToString() ?? string.Empty;
            session.Run().GetAwaiter().GetResult();
        }
        public virtual Task<bool> ValidateSender(MailAddress sender)
        {
            return Task.FromResult(true);
        }

        public virtual Task<(bool, string)> ValidateRecipient(MailAddress recipient, long messageSize)
        {
            return Task.FromResult((true, ""));
        }

        public virtual Task<bool> Message(Stream bodyStream, MailAddress sender, List<MailAddress> recipients)
        {
            return Task.FromResult(true);
        }

        public virtual bool ValidateCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public virtual X509Certificate? SelectCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate? remoteCertificate, string[] acceptableIssuers)
        {
            return null;
        }

    }

}
